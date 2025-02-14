using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using WebSocketState = System.Net.WebSockets.WebSocketState;

public class RateLimiter
{
    private readonly int m_MaxCalls; // 最大调用次数
    private readonly TimeSpan m_TimeWindow; // 时间窗口
    private readonly Queue<DateTime> m_CallTimestamps; // 调用时间记录
    private readonly SemaphoreSlim m_Semaphore; // 控制并发访问

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

public class BinanceWebSocketClient
{
    private const string            WebSocketUrl = "wss://fstream.binance.com/ws";
    private const int               MaxSubscriptionsPerConnection = 200; // 每个连接最多订阅 200 个 Streams
    private const int               MaxMessagesPerSecond = 10;           // 每秒最多发送 10 个订阅消息
    private const int               PongInterval = 15 * 60 * 1000;       // 客户端每 15 分钟发送 pong

    private ClientWebSocket         m_WebSocket;
    private CancellationTokenSource m_CancellationTokenSource;
    private List<string>            m_Subscriptions;      // 当前订阅的 Streams
    private RateLimiter             m_MessageRateLimiter; // 控制每秒最多发送 10 条消息

    public event Action<string>     OnMessageReceived; // 消息接收事件
    public event Action             OnDisconnected;    // 断开连接事件

    public BinanceWebSocketClient()
    {
        m_WebSocket = new ClientWebSocket();
        m_Subscriptions = new List<string>();
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
            Console.WriteLine("WebSocket connected.");
            
            // 启动接收消息和心跳维护任务
            _ = ReceiveMessagesAsync();
            _ = MaintainHeartbeatAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to WebSocket: {ex.Message}");
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
        Console.WriteLine("WebSocket disconnected.");
    }

    /// <summary>
    /// 订阅一个 Stream
    /// </summary>
    public async Task SubscribeAsync(string stream)
    {
        if (m_Subscriptions.Count >= MaxSubscriptionsPerConnection)
        {
            throw new InvalidOperationException("Maximum subscription limit reached.");
        }

        if (m_Subscriptions.Contains(stream))
        {
            Console.WriteLine($"Already subscribed to {stream}");
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

        m_Subscriptions.Add(stream);
        Console.WriteLine($"Subscribed to {stream}");
    }

    /// <summary>
    /// 取消订阅一个 Stream
    /// </summary>
    public async Task UnsubscribeAsync(string stream)
    {
        if (!m_Subscriptions.Contains(stream))
        {
            Console.WriteLine($"Not subscribed to {stream}");
            return;
        }

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

        m_Subscriptions.Remove(stream);
        Console.WriteLine($"Unsubscribed from {stream}");
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
                    Console.WriteLine("WebSocket closed by server.");
                    OnDisconnected?.Invoke();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving messages: {ex.Message}");
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
                Console.WriteLine("Pong sent to server.");
            }
        }
        catch (TaskCanceledException)
        {
            // 任务被取消，正常退出
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error maintaining heartbeat: {ex.Message}");
        }
    }
}

class Program
{
    static async Task Main1(string[] args)
    {
        var client = new BinanceWebSocketClient();

        client.OnMessageReceived += (message) =>
        {
            Console.WriteLine($"Message received: {message}");
        };

        client.OnDisconnected += () =>
        {
            Console.WriteLine("WebSocket disconnected. Attempting to reconnect...");
        };

        await client.ConnectAsync();

        // 订阅 BTCUSDT 的实时成交数据
        await client.SubscribeAsync("btcusdt@trade");
        await client.SubscribeAsync("arcusdt@trade");
        await client.SubscribeAsync("!ticker@arr");

        // 等待一段时间后取消订阅
        await Task.Delay(10000);
        await client.UnsubscribeAsync("!ticker@arr");
        await client.UnsubscribeAsync("arcusdt@trade");
        await client.UnsubscribeAsync("btcusdt@trade");

        // 断开连接
        await client.DisconnectAsync();
    }
    static async Task Main(string[] args)
    {
        // 创建一个速率限制器：每 5 秒最多调用 3 次
        var rateLimiter = new RateLimiter(3, TimeSpan.FromSeconds(5));

        // 模拟 10 次调用
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine($"Request {i + 1} started at {DateTime.Now:HH:mm:ss.fff}");
            await rateLimiter.WaitAsync(); // 等待速率限制器
            Console.WriteLine($"Request {i + 1} completed at {DateTime.Now:HH:mm:ss.fff}");
        }
    }
}