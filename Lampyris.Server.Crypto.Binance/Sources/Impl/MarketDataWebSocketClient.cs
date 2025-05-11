namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketState = System.Net.WebSockets.WebSocketState;

public class RateLimiter
{
    private readonly int             m_MaxCalls; // 最大调用次数
    private readonly TimeSpan        m_TimeWindow; // 时间窗口
    private readonly Queue<DateTime> m_CallTimestamps; // 调用时间记录
    private readonly SemaphoreSlim   m_Semaphore; // 控制并发访问

    public RateLimiter(int maxCalls, TimeSpan timeWindow)
    {
        if (maxCalls <= 0)
            throw new ArgumentException("Max calls must be greater than 0.", nameof(maxCalls));
        if (timeWindow.TotalMilliseconds <= 0)
            throw new ArgumentException("Time window must be greater than 0.", nameof(timeWindow));

        m_MaxCalls = maxCalls;
        m_TimeWindow = timeWindow;
        m_CallTimestamps = new Queue<DateTime>();
        m_Semaphore = new SemaphoreSlim(1, 1); // 用于线程安全
    }

    /// <summary>
    /// 尝试调用，如果超过限制则等待。
    /// </summary>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await m_Semaphore.WaitAsync(cancellationToken); // 确保线程安全
        try
        {
            DateTime now = DateTime.UtcNow;

            // 移除超出时间窗口的调用记录
            while (m_CallTimestamps.Count > 0 && (now - m_CallTimestamps.Peek()) > m_TimeWindow)
            {
                m_CallTimestamps.Dequeue();
            }

            // 如果调用次数已达到限制，则计算需要等待的时间
            if (m_CallTimestamps.Count >= m_MaxCalls)
            {
                DateTime oldestCall = m_CallTimestamps.Peek();
                TimeSpan waitTime = m_TimeWindow - (now - oldestCall);

                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime, cancellationToken); // 等待
                }
            }

            // 记录当前调用时间
            m_CallTimestamps.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            m_Semaphore.Release(); // 释放信号量
        }
    }
}

public class MarketDataWebSocketClient
{
    private const string WebSocketUrl = "wss://fstream.binance.com/ws";
    private const int    MaxSubscriptionsPerConnection = 200; // 每个连接最多订阅 200 个 Streams
    private const int    MaxMessagesPerSecond = 10;           // 每秒最多发送 10 个订阅消息
    private const int    PongInterval = 15 * 60 * 1000;       // 客户端每 15 分钟发送 pong

    private ClientWebSocket         m_WebSocket;
    private CancellationTokenSource m_CancellationTokenSource;
    private int                     m_SubscribedStream;      // 当前订阅的 Streams 数量
    private RateLimiter             m_MessageRateLimiter; // 控制每秒最多发送 10 条消息

    public event Action<string>     OnMessageReceived; // 消息接收事件
    public event Action             OnDisconnected;    // 断开连接事件

    public MarketDataWebSocketClient(ProxyInfo proxyInfo)
    {
        m_WebSocket = new ClientWebSocket();
        m_WebSocket.Options.Proxy = new WebProxy(proxyInfo.Address, proxyInfo.Port);
        m_MessageRateLimiter = new RateLimiter(MaxMessagesPerSecond, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// 连接到 WebSocket 服务器
    /// </summary>
    public async Task ConnectAsync()
    {
        m_CancellationTokenSource = new CancellationTokenSource();

        try
        {
            await m_WebSocket.ConnectAsync(new Uri(WebSocketUrl), m_CancellationTokenSource.Token);
            Logger.LogInfo("WebSocket connected.");

            // 启动接收消息和心跳维护任务
            _ = ReceiveMessagesAsync();
            _ = MaintainHeartbeatAsync();
        }
        catch (Exception ex)
        {
            Logger.LogInfo($"Error connecting to WebSocket: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 断开 WebSocket 连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        m_CancellationTokenSource?.Cancel();

        if (m_WebSocket.State == WebSocketState.Open)
        {
            await m_WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
        }

        m_WebSocket.Dispose();
        m_WebSocket = new ClientWebSocket();
        Logger.LogInfo("WebSocket disconnected.");
    }

    /// <summary>
    /// 订阅一个 Stream
    /// </summary>
    public async Task SubscribeAsync(string stream)
    {
        if (m_SubscribedStream >= MaxSubscriptionsPerConnection)
        {
            Logger.LogError("Maximum subscription limit reached.");
            return;
        }

        // 遵守订阅频率限制
        await m_MessageRateLimiter.WaitAsync();
        var message = new
        {
            method = "SUBSCRIBE",
            @params = new[] { stream },
            id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        string json = JsonSerializer.Serialize(message);
        await SendMessageAsync(json);

        Logger.LogInfo($"Subscribed to {stream}");
    }

    /// <summary>
    /// 取消订阅一个 Stream
    /// </summary>
    public async Task UnsubscribeAsync(string stream)
    {
        // 遵守订阅频率限制
        await m_MessageRateLimiter.WaitAsync();
        var message = new
        {
            method = "UNSUBSCRIBE",
            @params = new[] { stream },
            id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        string json = JsonSerializer.Serialize(message);
        await SendMessageAsync(json);

        Logger.LogInfo($"Unsubscribed from {stream}");
    }

    /// <summary>
    /// 发送消息到 WebSocket
    /// </summary>
    private async Task SendMessageAsync(string message)
    {
        if (m_WebSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }

        var buffer = Encoding.UTF8.GetBytes(message);
        await m_WebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, m_CancellationTokenSource.Token);
    }

    /// <summary>
    /// 接收 WebSocket 消息
    /// </summary>
    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 1024];

        try
        {
            while (m_WebSocket.State == WebSocketState.Open)
            {
                var result = await m_WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), m_CancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived?.Invoke(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Logger.LogInfo("Market Data WebSocket Client closed by server.");
                    OnDisconnected?.Invoke();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogInfo($"Error receiving messages: {ex.Message}");
            OnDisconnected?.Invoke();
        }
    }

    /// <summary>
    /// 维护心跳（发送 pong 帧）
    /// </summary>
    private async Task MaintainHeartbeatAsync()
    {
        try
        {
            while (m_WebSocket.State == WebSocketState.Open)
            {
                await Task.Delay(PongInterval, m_CancellationTokenSource.Token);

                // 发送 pong 帧
                var pongMessage = Encoding.UTF8.GetBytes("pong");
                await m_WebSocket.SendAsync(new ArraySegment<byte>(pongMessage), WebSocketMessageType.Text, true, m_CancellationTokenSource.Token);
                Logger.LogInfo("Pong sent to server.");
            }
        }
        catch (TaskCanceledException)
        {
            // 任务被取消，正常退出
        }
        catch (Exception ex)
        {
            Logger.LogInfo($"Error maintaining heartbeat: {ex.Message}");
        }
    }

    /// <summary>
    /// 每隔symbol订阅信息
    /// </summary>
    private class PerSymbolSubscriptionInfo
    {
        public bool             Trade;
        public bool             Depth;
        public HashSet<BarSize> KLine;
        public bool             MarkPrice;
    }

    private bool m_MarketTickerSubscribed;

    /// <summary>
    /// 存储了每个symbol对应的订阅信息的数据字典
    /// </summary>
    private Dictionary<string, PerSymbolSubscriptionInfo> m_PerSymbolSubscriptionInfoMap = new();

    private PerSymbolSubscriptionInfo GetOrCreateSubscriptionInfo(string symbol)
    {
        if(m_PerSymbolSubscriptionInfoMap.ContainsKey(symbol))
        {
            return m_PerSymbolSubscriptionInfoMap[symbol];
        }

        var subscriptionInfo = new PerSymbolSubscriptionInfo();
        m_PerSymbolSubscriptionInfoMap [symbol] = subscriptionInfo;
        return subscriptionInfo;
    }

    public async Task SubscribeTicker()
    {
        if (m_MarketTickerSubscribed)
        {
            Logger.LogWarning("The market ticker data has already been subscribed.");
            return;
        }
        await SubscribeAsync(MarketDataWebSocketStream.MARKET_TICKER);
        m_MarketTickerSubscribed = true;
    }

    public async Task CancelSubscribeTicker()
    {
        if (!m_MarketTickerSubscribed)
        {
            Logger.LogWarning("The market ticker data has not been subscribed.");
            return;
        }
        await UnsubscribeAsync(MarketDataWebSocketStream.MARKET_TICKER);
        m_MarketTickerSubscribed = false;
    }

    public async Task SubscribeTrade(string symbol)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if(subscriptionInfo.Trade)
        {
            Logger.LogWarning($"Trade data of Symbol \"{symbol}\" has already been subscribed.");
        }
        await SubscribeAsync(string.Format(MarketDataWebSocketStream.TRADE,symbol));
        subscriptionInfo.Trade = true;
    }

    public async Task CancelSubscribeTrade(string symbol)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if (!subscriptionInfo.Trade)
        {
            Logger.LogWarning($"Trade data of Symbol \"{symbol}\" has not been subscribed.");
        }
        await UnsubscribeAsync(string.Format(MarketDataWebSocketStream.TRADE, symbol));
        subscriptionInfo.Trade = false;
    }

    public async Task SubscribeDepth(string symbol)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if (subscriptionInfo.Depth)
        {
            Logger.LogWarning($"Depth data of Symbol \"{symbol}\" has already been subscribed.");
        }
        await SubscribeAsync(string.Format(MarketDataWebSocketStream.DEPTH, symbol));
        subscriptionInfo.Depth = true;
    }

    public async Task CancelSubscribeDepth(string symbol)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if (!subscriptionInfo.Depth)
        {
            Logger.LogWarning($"Depth data of Symbol \"{symbol}\" has not been subscribed.");
        }
        await UnsubscribeAsync(string.Format(MarketDataWebSocketStream.DEPTH, symbol));
        subscriptionInfo.Depth = false;
    }

    public async Task SubscribeKLine(string symbol, BarSize barSize)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if (subscriptionInfo.KLine.Contains(barSize))
        {
            Logger.LogWarning($"KLine data of Symbol \"{symbol}\", interval = {barSize.ToParamString()} has already been subscribed.");
        }
        await SubscribeAsync(string.Format(MarketDataWebSocketStream.KLINE, symbol));
        subscriptionInfo.KLine.Add(barSize);
    }

    public async Task CancelSubscribeKLine(string symbol, BarSize barSize)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if (!subscriptionInfo.KLine.Contains(barSize))
        {
            Logger.LogWarning($"KLine data of Symbol \"{symbol}\" interval = {barSize.ToParamString()} has not been subscribed.");
        }
        await UnsubscribeAsync(string.Format(MarketDataWebSocketStream.KLINE, symbol));
        subscriptionInfo.KLine.Remove(barSize);
    }

    public async Task SubscribeMarkPrice(string symbol)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if (subscriptionInfo.MarkPrice)
        {
            Logger.LogWarning($"Mark Price data of Symbol \"{symbol}\" has already been subscribed.");
        }
        await SubscribeAsync(string.Format(MarketDataWebSocketStream.MARK_PRICE, symbol));
        subscriptionInfo.MarkPrice = true;
    }

    public async Task CancelSubscribeMarkPrice(string symbol)
    {
        var subscriptionInfo = GetOrCreateSubscriptionInfo(symbol);
        if (!subscriptionInfo.MarkPrice)
        {
            Logger.LogWarning($"Mark Price data of Symbol \"{symbol}\" has not been subscribed.");
        }
        await UnsubscribeAsync(string.Format(MarketDataWebSocketStream.MARK_PRICE, symbol));
        subscriptionInfo.MarkPrice = false;
    }
}