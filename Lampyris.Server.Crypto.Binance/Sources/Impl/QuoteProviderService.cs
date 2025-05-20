using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures.Socket;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Objects.Sockets;
using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using System.Collections.Concurrent;
using System.IO.Compression;

namespace Lampyris.Server.Crypto.Binance;

// 单个IP可以有10个WebSocket对象
// 每个WebSocket对象最多有200个连接

// 需要订阅的行情数据有:
// 首先，Ticker占1个WebSocket对象
// 每个symbol 需要 1m + 15m + 1D数据 + trade数据
// 即:每个symbol需要占用4个WebSocket流
// 对于剩下9个WebSocket对象最多只能订阅9 * 200 / 4 = 450个symbol的数据
// 目前symbol总数大概是450个，为了防止以后symbol数量超过450个而导致不够用，
// 于是这里最少需要2个IP地址来进行运作, 2个IP最多能订阅450 + 500 = 950个symbol的数据

public class QuoteWebSocketClientAllocator
{
    private class WebSocketClientInfo
    {
        public BinanceSocketClient SocketClient;
        public int                 StreamUsedCount;
    }

    private readonly int              COMMON_STREAM_COUNT = 2;
    private readonly int              PER_IP_WEBSOCKETCOUNT = 10;
    private readonly int              PER_WEBSOCKET_CLIENT_STREAM_COUNT = 200;
    private readonly int              PER_SYMBOL_STREAM_REQUIRMENT = 4;
    private Dictionary<string,int>    m_Symbol2WebSocketInfoIndex = new Dictionary<string,int>();
    private List<WebSocketClientInfo> m_WebSocketClientInfoList = new List<WebSocketClientInfo>();
    private ProxyProvideService       m_ProxyProvideService;

    private WebSocketClientInfo CreateWebSocketClient()
    {
        // 需要使用的代理信息的索引
        int count = m_WebSocketClientInfoList.Count;
        int proxyIndex = count / PER_IP_WEBSOCKETCOUNT;
        var proxyInfo = m_ProxyProvideService.Get(proxyIndex);

        if(proxyInfo == null)
        {
            throw new InvalidProgramException("Failed to allocate create WebSocket client: proxy info is invalid");
        }

        var info = new WebSocketClientInfo()
        {
            SocketClient = new BinanceSocketClient(options =>
            {
                options.Proxy = new CryptoExchange.Net.Objects.ApiProxy("http://" + proxyInfo.Address, proxyInfo.Port);
            }),
            StreamUsedCount = 0,
        };

        m_WebSocketClientInfoList.Add(info);
        return info;
    }
    
    public QuoteWebSocketClientAllocator(ProxyProvideService proxyProvideService, IEnumerable<string> symbols)
    {
        m_ProxyProvideService = proxyProvideService;
        WebSocketClientInfo? info = CreateWebSocketClient();
        // Ticker数据和 MarkPrice数据的订阅 占了2个stream
        info.StreamUsedCount += COMMON_STREAM_COUNT;

        int index = 0;
        foreach (string symbol in symbols)
        {
            if(info.StreamUsedCount + PER_SYMBOL_STREAM_REQUIRMENT > PER_WEBSOCKET_CLIENT_STREAM_COUNT) // 计算stream占用数是否超过最大值
            {
                // 如果是则创建
                info = CreateWebSocketClient();
                index++;
            }

            m_Symbol2WebSocketInfoIndex[symbol] = index;
            info.StreamUsedCount += PER_SYMBOL_STREAM_REQUIRMENT;
        }
    }

    public void HandleSymbolChange(IEnumerable<string> removedSymbols, IEnumerable<string> addedSymbols)
    {
        if (removedSymbols.Any() || addedSymbols.Any())
        {
            List<string> removedSymbolsList = removedSymbols.ToList();
            List<string> addedSymbolsList = addedSymbols.ToList();

            int count = Math.Min(removedSymbolsList.Count, addedSymbolsList.Count);

            for (int i = 0; i < count; i++)
            {
                int index = m_Symbol2WebSocketInfoIndex[removedSymbolsList[i]];
                m_Symbol2WebSocketInfoIndex.Remove(removedSymbolsList[i]);
                m_Symbol2WebSocketInfoIndex[addedSymbolsList[i]] = index;
            }

            if(removedSymbolsList.Count > count)
            {
                for (int i = count; i < count; i++)
                {
                    m_Symbol2WebSocketInfoIndex.Remove(removedSymbolsList[i]);
                }
            }
            if (addedSymbolsList.Count > count)
            {
                int index = 0;
                WebSocketClientInfo info = m_WebSocketClientInfoList[index];
                for (int i = count; i < count; i++)
                {
                    while(info != null && info.StreamUsedCount + PER_SYMBOL_STREAM_REQUIRMENT > PER_WEBSOCKET_CLIENT_STREAM_COUNT)
                    {
                        index++;
                        info = m_WebSocketClientInfoList[index];
                    }
                    if(info == null)
                    {
                        Logger.LogWarning("Unable to handle added symbol: insufficient WebSocket stream resource");
                        break;
                    }
                    info.StreamUsedCount += PER_SYMBOL_STREAM_REQUIRMENT;
                }
            }
        }
    }

    public BinanceSocketClient GetWebSocketClient(string symbol)
    {
        return m_WebSocketClientInfoList[m_Symbol2WebSocketInfoIndex[symbol]].SocketClient;
    }

    public BinanceSocketClient GetCommonWebSocketClient()
    {
        return m_WebSocketClientInfoList[0].SocketClient;
    }
}

[Component]
public class QuoteProviderService : AbstractQuoteProviderService
{
    [Autowired]
    private ProxyProvideService m_ProxyProvideService;

    [Autowired]
    private HistoricalDataDownloader m_HistoricalDataDownloader;

    private List<BinanceRestClient> m_RestClientList = new();
    private QuoteWebSocketClientAllocator m_WebSocketClientAllocator;

    // 存储订阅的句柄，用于取消订阅
    private ConcurrentDictionary<string, Dictionary<KlineInterval, UpdateSubscription>> m_KlineSubscriptions = new();
    private ConcurrentDictionary<string, UpdateSubscription> m_TradeSubscriptions = new();
    private UpdateSubscription m_MarkPriceSubscription;

    public override void OnStart()
    {
        m_RestClientList = new List<BinanceRestClient>(m_ProxyProvideService.ProxyCount);

        for (int i = 0; i < m_ProxyProvideService.ProxyCount; i++)
        {
            var proxyInfo = m_ProxyProvideService.Get(i);
            if (proxyInfo == null)
            {
                throw new InvalidProgramException("Failed to allocate create REST client: proxy info is invalid");
            }
            BinanceRestClient client = new BinanceRestClient(options =>
            {
                options.Proxy = new CryptoExchange.Net.Objects.ApiProxy("http://" + proxyInfo.Address, proxyInfo.Port);
            });

            m_RestClientList.Add(client);
        }

        base.OnStart();
    }

    /// <summary>
    /// 交替使用BinanceRestClient对象所记录的索引
    /// </summary>
    private int m_RestClientIndex = -1;
    private BinanceRestClient m_RestClient
    {
        get
        {
            return m_RestClientList.Count > 0 ? m_RestClientList[(m_RestClientIndex + 1 % m_RestClientList.Count)] : null;
        }
    }

    private BinanceSocketClient m_CommonSocketClient
    {
        get
        {
            return m_WebSocketClientAllocator != null ? m_WebSocketClientAllocator.GetCommonWebSocketClient() : null;
        }
    }

    protected override async Task APISubscriptionAllImpl()
    {
        // 获取全体 USDT 永续合约的 symbol 列表
        var symbols = GetSymbolList();
        if (symbols != null)
        {
            foreach (var symbol in symbols)
            {
                m_Symbols.Add(symbol);
            }

            // 初始化订阅
            var task1 = SubscribeToTickerUpdatesForAllSymbols();
            var task2 = SubscribeToTradeUpdatesForAllSymbols();
            var task3 = SubscribeToMarkPriceUpdatesForAllSymbols();

            await Task.WhenAll(task1, task2, task3);
        }
    }

    /// <summary>
    /// 更新全体 USDT 永续合约的 symbol 列表 + 上架时间
    /// </summary> 
    protected override void APIUpdateUsdtFuturesSymbolsImpl()
    {
        var exchangeInfoResult = m_RestClient.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync().Result;
        if (exchangeInfoResult.Success)
        {
            m_Symbols.Clear();

            var result = exchangeInfoResult.Data.Symbols
                .Where(s => s.ContractType == ContractType.Perpetual && s.QuoteAsset == "USDT");

            foreach(var symbolInfo in result)
            {
                m_Symbols.Add(symbolInfo.Name);
                if (!m_Symbol2TradeRuleMap.ContainsKey(symbolInfo.Name))
                {
                    m_Symbol2TradeRuleMap[symbolInfo.Name] = new SymbolTradeRule();
                }
                m_Symbol2TradeRuleMap[symbolInfo.Name] = Converter.ToSymbolTradeRule(symbolInfo, m_Symbol2TradeRuleMap[symbolInfo.Name]);
            }

            if (m_WebSocketClientAllocator == null)
            {
                m_WebSocketClientAllocator = new QuoteWebSocketClientAllocator(m_ProxyProvideService,m_Symbols);
            }
        }
    }

    #region Ticker数据
    /// <summary>
    /// 订阅全体 symbol 的 Ticker 数据
    /// </summary>
    private async Task SubscribeToTickerUpdatesForAllSymbols()
    {
        var result = await m_RestClient.UsdFuturesApi.ExchangeData.GetTickersAsync();
        OnTickerUpdateImpl(result.Data, DateTime.UtcNow);
        await m_CommonSocketClient.UsdFuturesApi.ExchangeData.SubscribeToAllTickerUpdatesAsync(OnTickerUpdate);
    }

    private void OnTickerUpdateImpl(IEnumerable<IBinance24HPrice> dataList, DateTime dateTime)
    {
        long timestamp = DateTimeUtil.ToUnixTimestampMilliseconds(dateTime);
        foreach (var rawTickerData in dataList)
        {
            if (!rawTickerData.Symbol.EndsWith("USDT"))
            {
                continue;
            }

            QuoteTickerData? quoteTickerData = null;
            if (m_QuoteTickerDataMap.ContainsKey(rawTickerData.Symbol))
            {
                quoteTickerData = m_QuoteTickerDataMap[rawTickerData.Symbol];
            }
            else
            {
                quoteTickerData = new QuoteTickerData();
                m_QuoteTickerDataList.Add(quoteTickerData);
            }
            m_QuoteTickerDataMap[rawTickerData.Symbol] = Converter.ToQuoteTickerData(timestamp, rawTickerData, quoteTickerData);
        }

        PostProcessTickerData();

        if (OnTickerUpdated != null)
        {
            OnTickerUpdated(m_QuoteTickerDataList);
        }
    }

    private void OnTickerUpdate(DataEvent<IEnumerable<IBinance24HPrice>> dataEvent)
    {
        if(dataEvent.Data != null && dataEvent.DataTime != null)
        {
            OnTickerUpdateImpl(dataEvent.Data, dataEvent.DataTime.Value);
        }
    }

    #endregion

    private HttpClient m_HttpClient = new HttpClient();

    #region K线数据
    public override List<QuoteCandleData> APIQueryCandleDataImpl(string symbol, BarSize barSize, DateTime? startTime, DateTime? endTime, int n = -1)
    {
        // 检查输入参数
        if (string.IsNullOrEmpty(symbol))
            throw new ArgumentException("Symbol cannot be null or empty.");

        // 将 BarSize 转换为 Binance 的时间间隔
        KlineInterval interval = Converter.ConvertBarSize(barSize);

        // Binance 最大单次查询数量
        const int maxQueryLimit = 1500;

        // 如果 n = -1，则默认取 1500
        n = n == -1 ? maxQueryLimit : n;

        // 初始化结果列表
        List<QuoteCandleData> result = new List<QuoteCandleData>();

        // 根据不同情况处理
        if (startTime == null && endTime == null)
        {
            // 情况 1：单线程请求最近 n 根
            result = QueryKlines(symbol, interval, null, null, n);
        }
        else
        {
            DateTime actualEndTime   = DateTime.MinValue;
            DateTime actualStartTime = DateTime.MaxValue;

            if (startTime == null && endTime != null)
            {
                // 情况 2：多线程请求以 endTime 为结束的 n 根 k 线
                actualEndTime = endTime.Value;
                actualStartTime = actualEndTime.AddSeconds(-(int)interval * n);
            }
            else if (startTime != null && endTime == null)
            {
                // 情况 3：多线程请求以 startTime 为开始的 n 根 k 线
                actualStartTime = startTime.Value;
                actualEndTime = actualStartTime.AddSeconds((int)interval * n);
            }
            else if (startTime != null && endTime != null)
            {
                // 情况 4：多线程请求所有以 startTime 为开始，以 endTime 为结束的 k 线
                actualStartTime = startTime.Value;
                actualEndTime = endTime.Value;

                // 如果时间范围内的 k 线数量大于 n，则调整 startTime
                n = 1 + Math.Max(0, (int)Math.Ceiling((double)(actualEndTime - actualStartTime).TotalSeconds / (double)interval));
            }

            // 当前日期减去2天的数据,都会存到Binance数据服务器里
            // 所以需要分段处理，一部分从数据服务器里中下载，一部分根据API下载
            DateTime dataVisionDownloadStartTime = new DateTime(actualStartTime.Year, actualStartTime.Month, actualStartTime.Day);
            DateTime dataVisionDownloadEndTime = actualEndTime.AddDays(-2);
            dataVisionDownloadEndTime = new DateTime(dataVisionDownloadEndTime.Year, dataVisionDownloadEndTime.Month, dataVisionDownloadEndTime.Day);

            if(dataVisionDownloadStartTime < new DateTime(2020,1,1))
            {
                dataVisionDownloadStartTime = new DateTime(2020, 1, 1);
            }

            string dirPath = $"F:/QuoteDB/{symbol}/{barSize.ToParamString()}";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            // 创建日期列表
            List<DateTime> dateList = new List<DateTime>();
            while (dataVisionDownloadStartTime <= dataVisionDownloadEndTime)
            {
                dateList.Add(dataVisionDownloadStartTime);
                dataVisionDownloadStartTime = dataVisionDownloadStartTime.AddDays(1);
            }

            // 并发下载任务
            var tasks = dateList.Select(date => Task.Run(async () =>
            {
                string barSizeString = barSize.ToParamString().ToLower();
                string formattedDate = date.ToString("yyyy-MM-dd");
                string fileName = $"{symbol}-{barSizeString}-{formattedDate}.csv";
                string filePath = Path.Combine(dirPath, fileName);

                // 如果文件已存在，跳过下载
                if (File.Exists(filePath))
                {
                    return;
                }

                string baseUrl = "https://data.binance.vision/data/futures/um/daily/klines";
                string url = $"{baseUrl}/{symbol}/{barSizeString}/{symbol}-{barSizeString}-{formattedDate}.zip";
                string tempFilePath = Path.GetTempFileName();

                try
                {
                    // 下载 ZIP 文件
                    using (var response = await m_HttpClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }

                    // 解压 ZIP 文件
                    ZipFile.ExtractToDirectory(tempFilePath, dirPath);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                finally
                {
                    // 清理临时文件
                    if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                }
            }));

            // 等待所有任务完成
            Task.WaitAll(tasks.ToArray());
            result = new List<QuoteCandleData>(); //  QueryKlinesMultiThread(symbol, interval, actualStartTime, actualEndTime, n);
        }
        return result;
    }

    /// <summary>
    /// 单线程查询 K 线数据
    /// </summary>
    private List<QuoteCandleData> QueryKlines(string symbol, KlineInterval interval, DateTime? startTime, DateTime? endTime, int limit)
    {
        var client = m_RestClient;
        var response = client.UsdFuturesApi.ExchangeData.GetKlinesAsync(
            symbol: symbol,
            interval: interval,
            startTime: startTime,
            endTime: endTime,
            limit: limit
        ).Result;

        if (!response.Success || response.Data == null)
        {
            throw new Exception($"Failed to query Kline data: {response.Error?.Message}");
        }

        return response.Data.Select(kline => Converter.ConvertQuoteCandleData(kline)).ToList();
    }

    /// <summary>
    /// 多线程查询 K 线数据
    /// </summary>
    private List<QuoteCandleData> QueryKlinesMultiThread(string symbol, KlineInterval interval, DateTime startTime, DateTime endTime, int limit)
    {

        // Binance 最大单次查询数量
        const int maxQueryLimit = 1500;

        // 分段时间范围
        List<(DateTime Start, DateTime End)> timeRanges = new List<(DateTime, DateTime)>();

        DateTime currentStartTime = startTime;

        while (currentStartTime < endTime)
        {
            DateTime currentEndTime = currentStartTime.AddSeconds((int)interval * maxQueryLimit);
            if (currentEndTime > endTime)
                currentEndTime = endTime;

            timeRanges.Add((currentStartTime, currentEndTime));
            currentStartTime = currentEndTime;
        }

        // 多线程并发请求
        var tasks = timeRanges.Select(range =>
        {
            return Task.Run(() => QueryKlines(symbol, interval, range.Start, range.End, maxQueryLimit));
        });

        // 等待所有任务完成并合并结果
        Task.WaitAll(tasks.ToArray());
        List<QuoteCandleData> result = new List<QuoteCandleData>();
        foreach (var task in tasks)
        {
            result.AddRange(task.Result);
        }

        // 如果结果数量大于限制，则取最近的 n 条数据
        if (result.Count > limit)
        {
            result = result.TakeLast(limit).ToList();
        }

        return result;
    }

    /// <summary>
    /// 订阅指定 symbol的K 线数据
    /// </summary>
    private void SubscribeToKlineUpdates(string symbol, KlineInterval interval)
    {
        BinanceSocketClient socketClient = m_WebSocketClientAllocator.GetWebSocketClient(symbol);
        if (socketClient == null)
        {
            throw new NullReferenceException($"Unexpected null pointer for symbol \"{symbol}\"");
        }

        var subscriptionResult = socketClient.UsdFuturesApi.ExchangeData.SubscribeToKlineUpdatesAsync(symbol, interval, OnKlineUpdate).Result;
        if (subscriptionResult.Success)
        {
            if (!m_KlineSubscriptions.ContainsKey(symbol))
            {
                m_KlineSubscriptions[symbol] = new Dictionary<KlineInterval, UpdateSubscription>();
            }
            var subscription = m_KlineSubscriptions[symbol];
            if (subscription.ContainsKey(interval))
            {
                Logger.LogWarning($"Duplicated subscription for Symbol = \"{symbol}\", API Interval = {interval}");
                return;
            }
            m_KlineSubscriptions[symbol][interval] = subscriptionResult.Data;
        }
    }

    private void OnKlineUpdate(DataEvent<IBinanceStreamKlineData> dataEvent)
    {
        if (dataEvent != null && dataEvent.Data != null && dataEvent.Symbol != null)
        {
            var klineData = dataEvent.Data.Data;
            if(klineData != null)
            {
                m_CacheService.StorageCandleData(dataEvent.Symbol, Converter.ConvertBarSize(klineData.Interval), Converter.ConvertQuoteCandleData(klineData));
            }
        }
    }
    #endregion

    #region 交易数据

    public delegate void OnTradeUpdateHandlerDelegate(string symbol, QuoteTradeData quoteTradeData);

    public OnTradeUpdateHandlerDelegate OnTradeUpdateHandler;

    /// <summary>
    /// 订阅全体 symbol 的聚合交易数据
    /// </summary>
    private async Task SubscribeToTradeUpdatesForAllSymbols()
    {
        Task[] tasks = new Task[m_Symbols.Count];
        int index = 0;

        foreach (var symbol in m_Symbols)
        {
            tasks[index++] = Task.Run(() => SubscribeToTradeUpdate(symbol));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 订阅指定 symbol 的聚合交易数据
    /// </summary>
    private void SubscribeToTradeUpdate(string symbol)
    {
        BinanceSocketClient socketClient = m_WebSocketClientAllocator.GetWebSocketClient(symbol);
        if (socketClient == null)
        {
            throw new NullReferenceException($"Unexpected null pointer for symbol \"{symbol}\"");
        }

        var subscriptionResult = socketClient.UsdFuturesApi.ExchangeData.SubscribeToAggregatedTradeUpdatesAsync(symbol, OnTradeUpdate).Result;
        if (subscriptionResult.Success)
        {
            m_TradeSubscriptions[symbol] = subscriptionResult.Data;
        }
        else
        {
            Logger.LogError($"Failed to subscribe trade update for Symbol \"{symbol}\"");
        }
    }

    /// <summary>
    /// Binance 交易数据回调
    /// </summary>
    /// <param name="dataEvent"></param>
    private void OnTradeUpdate(DataEvent<BinanceStreamAggregatedTrade> dataEvent)
    {
        if (dataEvent.Data != null)
        {
            QuoteTradeData quoteTradeData = new QuoteTradeData();
            quoteTradeData.Price = dataEvent.Data.Price;
            quoteTradeData.Quantity = dataEvent.Data.Quantity;
            quoteTradeData.TradeTime = dataEvent.Data.TradeTime;
            quoteTradeData.BuyerIsMaker = dataEvent.Data.BuyerIsMaker;
        }
    }
    #endregion

    #region 标记价格
    /// <summary>
    /// 订阅全体 symbol 的标记价格数据
    /// </summary>
    private async Task SubscribeToMarkPriceUpdatesForAllSymbols()
    {
        var subscriptionResult = await m_CommonSocketClient.UsdFuturesApi.ExchangeData.SubscribeToAllMarkPriceUpdatesAsync(1000, OnMarkPriceUpdate);
        if (subscriptionResult.Success)
        {
            m_MarkPriceSubscription = subscriptionResult.Data;
        }
    }

    /// <summary>
    /// API事件-标记价格更新回调
    /// </summary>
    /// <param name="dataEvent"></param>
    private void OnMarkPriceUpdate(DataEvent<IEnumerable<BinanceFuturesUsdtStreamMarkPrice>> dataEvent)
    {
        if(dataEvent != null && dataEvent.Data != null)
        {
            foreach(var markPriceData in dataEvent.Data)
            {
                if (m_QuoteTickerDataMap.ContainsKey(markPriceData.Symbol))
                {
                    var quoteTickerData = m_QuoteTickerDataMap[markPriceData.Symbol];
                    quoteTickerData.MarkPrice = markPriceData.MarkPrice;
                    quoteTickerData.IndexPrice = markPriceData.IndexPrice;
                    quoteTickerData.NextFundingTime = DateTimeUtil.ToUnixTimestampMilliseconds(markPriceData.NextFundingTime);
                    quoteTickerData.FundingRate = markPriceData.FundingRate ?? 0.0m;
                }
            }
        }
    }
    #endregion

    #region API订阅
    /// <summary>
    /// 订阅单个 symbol 的所有数据
    /// </summary>
    protected override void APISubscribeToSymbolImpl(string symbol)
    {
        foreach (var barSize in m_ConcernedBarSizeList)
        {
            SubscribeToKlineUpdates(symbol, Converter.ConvertBarSize(barSize));
        }
        SubscribeToTradeUpdate(symbol);
    }

    /// <summary>
    /// 取消订阅单个 symbol 的所有数据
    /// </summary>
    protected override void APIUnsubscribeFromSymbolImpl(string symbol)
    {
        BinanceSocketClient socketClient = m_WebSocketClientAllocator.GetWebSocketClient(symbol);
        if (socketClient == null)
        {
            throw new NullReferenceException($"Unexpected null pointer for symbol \"{symbol}\"");
        }
        if (m_KlineSubscriptions.ContainsKey(symbol))
        {
            foreach (var barSize in m_ConcernedBarSizeList)
            {
                var interval = Converter.ConvertBarSize(barSize);
                if (m_KlineSubscriptions[symbol].ContainsKey(interval))
                {
                    socketClient.UsdFuturesApi.UnsubscribeAsync(m_KlineSubscriptions[symbol][interval]);
                    m_KlineSubscriptions[symbol].Remove(interval);
                }
            }
            m_KlineSubscriptions.Remove(symbol,out var _);
        }

        if (m_TradeSubscriptions.ContainsKey(symbol))
        {
            socketClient.UsdFuturesApi.UnsubscribeAsync(m_TradeSubscriptions[symbol]);
            m_TradeSubscriptions.Remove(symbol, out var _);
        }
    }

    public override void APISubscribeCandleData(string symbol, BarSize barSize)
    {
        SubscribeToKlineUpdates(symbol, Converter.ConvertBarSize(barSize));
    }

    protected override DateTime GetAPIServerDateTimeImpl()
    {
        var result = m_RestClient.UsdFuturesApi.ExchangeData.GetServerTimeAsync().Result;
        if (!result.Success)
        {
            return DateTime.UtcNow;
        }
        return result.Data;
    }
    #endregion
}
