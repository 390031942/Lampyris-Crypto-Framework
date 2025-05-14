using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures.Socket;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Objects.Sockets;
using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using System.Collections.Concurrent;

namespace Lampyris.Server.Crypto.Binance;

// 单个IP可以有10个WebSocket对象
// 每个WebSocket对象最多有200个连接

// 需要订阅的行情数据有:
// 首先，Ticker占1个WebSocket对象
// 每个symbol 需要 1m + 15m + 1D数据 + trade数据 + 标记价格更新
// 即:每个symbol需要占用5个WebSocket流
// 对于剩下9个WebSocket对象最多只能订阅9 * 200 / 5 = 360个symbol的数据
// 于是这里最少需要2个IP地址来进行运作, 2个IP最多能订阅360+400 = 720个symbol的数据

public class QuoteProviderService : AbstractQuoteProviderService
{
    [Autowired]
    private ProxyProvideService m_ProxyProvideService;

    private BinanceRestClient m_RestClient = new BinanceRestClient();
    private BinanceSocketClient m_SocketClient = new BinanceSocketClient();

    // 存储订阅的句柄，用于取消订阅
    private ConcurrentDictionary<string, Dictionary<KlineInterval, UpdateSubscription>> m_KlineSubscriptions = new();
    private ConcurrentDictionary<string, UpdateSubscription> m_TradeSubscriptions = new();
    private ConcurrentDictionary<string, UpdateSubscription> m_MarkPriceSubscriptions = new();

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
            m_Symbol2OnBoardTime.Clear();

            var result = exchangeInfoResult.Data.Symbols
                .Where(s => s.ContractType == ContractType.Perpetual && s.QuoteAsset == "USDT");

            foreach(var symbolInfo in result)
            {
                m_Symbols.Add(symbolInfo.Name);
                m_Symbol2OnBoardTime[symbolInfo.Name] = symbolInfo.ListingDate;
            }
        }
    }

    #region Ticker数据
    /// <summary>
    /// 订阅全体 symbol 的 Ticker 数据
    /// </summary>
    private async Task SubscribeToTickerUpdatesForAllSymbols()
    {
        await m_SocketClient.UsdFuturesApi.ExchangeData.SubscribeToAllTickerUpdatesAsync(OnTickerUpdate);
    }

    private void OnTickerUpdate(DataEvent<IEnumerable<IBinance24HPrice>> dataEvent)
    {
        if(dataEvent.Data != null && dataEvent.DataTime != null)
        {
            long timestamp = DateTimeUtil.ToUnixTimestampMilliseconds(dataEvent.DataTime.Value);
            foreach(var rawTickerData in dataEvent.Data)
            {
                QuoteTickerData? quoteTickerData = null;
                if(m_QuoteTickerDataMap.ContainsKey(rawTickerData.Symbol)) {
                    quoteTickerData = m_QuoteTickerDataMap[rawTickerData.Symbol];
                } 
                else
                {
                    quoteTickerData = new QuoteTickerData();
                }
                m_QuoteTickerDataMap[rawTickerData.Symbol] = Converter.ToQuoteTickerData(timestamp, rawTickerData, quoteTickerData);
            }

            PostProcessTickerData();
            OnTickerUpdated(m_QuoteTickerDataList);
        }
    }

    #endregion

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

        // 处理 startTime 和 endTime 的默认值
        DateTime actualStartTime = startTime ?? DateTime.UtcNow.AddYears(-1); // Binance 支持的最早时间
        DateTime actualEndTime = endTime ?? DateTime.UtcNow;

        if (actualStartTime >= actualEndTime)
            throw new ArgumentException("StartTime must be earlier than EndTime.");

        // 初始化结果列表
        List<QuoteCandleData> result = new List<QuoteCandleData>();

        // 当前查询的时间范围分段
        List<(DateTime Start, DateTime End)> timeRanges = new List<(DateTime, DateTime)>();
        DateTime currentStartTime = actualStartTime;

        while (currentStartTime < actualEndTime)
        {
            DateTime currentEndTime = currentStartTime.AddMilliseconds(interval.ToMilliseconds() * maxQueryLimit);
            if (currentEndTime > actualEndTime)
                currentEndTime = actualEndTime;

            timeRanges.Add((currentStartTime, currentEndTime));
            currentStartTime = currentEndTime;
        }

        // 配置多个 BinanceClient，每个使用不同的代理服务器
        var proxyList = new List<string>
        {
            "http://proxy1.example.com:8080",
            "http://proxy2.example.com:8080",
            "http://proxy3.example.com:8080"
        };

        var clients = proxyList.Select(proxy => CreateBinanceClientWithProxy(proxy)).ToList();

        // 多线程并发请求
        var tasks = timeRanges.Select((range, index) =>
        {
            var client = clients[index % clients.Count]; // 轮询使用不同的客户端
            return Task.Run(() =>
            {
                var response = client.UsdFuturesApi.ExchangeData.GetKlinesAsync(
                    symbol: symbol,
                    interval: interval,
                    startTime: range.Start,
                    endTime: range.End,
                    limit: maxQueryLimit
                ).Result;

                if (!response.Success || response.Data == null)
                {
                    throw new Exception($"Failed to query Kline data: {response.Error?.Message}");
                }

                // 转换 Binance K 线数据为 QuoteCandleData
                return response.Data.Select(kline => new QuoteCandleData
                {
                    OpenTime = kline.OpenTime,
                    Open = kline.OpenPrice,
                    High = kline.HighPrice,
                    Low = kline.LowPrice,
                    Close = kline.ClosePrice,
                    Volume = kline.Volume
                }).ToList();
            });
        });

        // 等待所有任务完成并合并结果
        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            result.AddRange(task.Result);
        }

        // 如果 n > 0，则返回最近的 n 条数据
        if (n > 0 && result.Count > n)
        {
            result = result.TakeLast(n).ToList();
        }

        return result;
    }

    // 辅助方法：创建带有代理的 BinanceClient
    private BinanceRestClient CreateBinanceClientWithProxy(string proxyUrl)
    {
        return new BinanceRestClient(clientOptions);
    }
    /// <summary>
    /// 订阅全体symbol的K线数据
    /// </summary>
    private void SubscribeToKlineUpdatesForAllSymbols(KlineInterval interval)
    {
        foreach (var symbol in m_Symbols)
        {
            SubscribeToKlineUpdates(symbol, interval);
        }
    }

    /// <summary>
    /// 订阅指定 symbol的K 线数据
    /// </summary>
    private void SubscribeToKlineUpdates(string symbol, KlineInterval interval)
    {
        var subscriptionResult = m_SocketClient.UsdFuturesApi.ExchangeData.SubscribeToKlineUpdatesAsync(symbol, interval, OnKlineUpdate).Result;
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
        var subscriptionResult = await m_SocketClient.UsdFuturesApi.ExchangeData.SubscribeToAggregatedTradeUpdatesAsync(m_Symbols, OnTradeUpdate);
        if (subscriptionResult.Success)
        {
            foreach (var symbol in m_Symbols)
            {
                m_TradeSubscriptions[symbol] = subscriptionResult.Data;
            }
        }
        else
        {
            Logger.LogError($"Failed to subscribe trade update for all symbols");
        }
    }

    /// <summary>
    /// 订阅指定 symbol 的聚合交易数据
    /// </summary>
    private void SubscribeToTradeUpdates(string symbol)
    {
        var subscriptionResult = m_SocketClient.UsdFuturesApi.ExchangeData.SubscribeToAggregatedTradeUpdatesAsync(symbol, OnTradeUpdate).Result;
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
        var subscriptionResult = await m_SocketClient.UsdFuturesApi.ExchangeData.SubscribeToMarkPriceUpdatesAsync(m_Symbols, 1000, OnMarkPriceUpdate);
        if (subscriptionResult.Success)
        {
            foreach (var symbol in m_Symbols)
            {
                m_MarkPriceSubscriptions[symbol] = subscriptionResult.Data;
            }
        }
    }

    /// <summary>
    /// 订阅指定 symbol 的标记价格数据
    /// </summary>
    private void SubscribeToMarkPriceUpdates(string symbol)
    {
        var subscriptionResult = m_SocketClient.UsdFuturesApi.ExchangeData.SubscribeToMarkPriceUpdatesAsync(symbol, 1000, OnMarkPriceUpdate).Result;
        if (subscriptionResult.Success)
        {
            m_MarkPriceSubscriptions[symbol] = subscriptionResult.Data;
        }
    }

    /// <summary>
    /// API事件-标记价格更新回调
    /// </summary>
    /// <param name="dataEvent"></param>
    private void OnMarkPriceUpdate(DataEvent<BinanceFuturesUsdtStreamMarkPrice> dataEvent)
    {
        if(dataEvent != null && dataEvent.Data != null)
        {
            if (m_QuoteTickerDataMap.ContainsKey(dataEvent.Data.Symbol))
            {
                var quoteTickerData = m_QuoteTickerDataMap[dataEvent.Data.Symbol];
                quoteTickerData.MarkPrice = dataEvent.Data.MarkPrice;
                quoteTickerData.IndexPrice = dataEvent.Data.IndexPrice;
                quoteTickerData.NextFundingTime = DateTimeUtil.ToUnixTimestampMilliseconds(dataEvent.Data.NextFundingTime);
                quoteTickerData.FundingRate = dataEvent.Data.FundingRate ?? 0.0m;
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
        SubscribeToTradeUpdates(symbol);
        SubscribeToMarkPriceUpdates(symbol);
    }

    /// <summary>
    /// 取消订阅单个 symbol 的所有数据
    /// </summary>
    protected override void APIUnsubscribeFromSymbolImpl(string symbol)
    {
        if (m_KlineSubscriptions.ContainsKey(symbol))
        {
            foreach (var barSize in m_ConcernedBarSizeList)
            {
                var interval = Converter.ConvertBarSize(barSize);
                if (m_KlineSubscriptions[symbol].ContainsKey(interval))
                {
                    m_SocketClient.UsdFuturesApi.UnsubscribeAsync(m_KlineSubscriptions[symbol][interval]);
                    m_KlineSubscriptions[symbol].Remove(interval);
                }
            }
            m_KlineSubscriptions.Remove(symbol,out var _);
        }

        if (m_TradeSubscriptions.ContainsKey(symbol))
        {
            m_SocketClient.UsdFuturesApi.UnsubscribeAsync(m_TradeSubscriptions[symbol]);
            m_TradeSubscriptions.Remove(symbol, out var _);
        }

        if (m_MarkPriceSubscriptions.ContainsKey(symbol))
        {
            m_SocketClient.UsdFuturesApi.UnsubscribeAsync(m_MarkPriceSubscriptions[symbol]);
            m_MarkPriceSubscriptions.Remove(symbol, out var _);
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
