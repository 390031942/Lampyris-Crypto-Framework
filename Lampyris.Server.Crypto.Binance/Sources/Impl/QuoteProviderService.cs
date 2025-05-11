using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures.Socket;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Objects.Sockets;
using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using MySqlX.XDevAPI;
using System.Collections.Concurrent;

namespace Lampyris.Server.Crypto.Binance;

public class QuoteProviderService : AbstractQuoteProviderService
{
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
            m_Symbols.UnionWith(exchangeInfoResult.Data.Symbols
                .Where(s => s.ContractType == ContractType.Perpetual && s.QuoteAsset == "USDT")
                .Select(s => s.Name));
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

            // 后处理
        }
    }
    #endregion

    #region K线数据
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

    public override List<QuoteCandleData> APIQueryCandleDataImpl(string symbol, BarSize barSize, DateTime startTime, DateTime endTime)
    {
        // Binance 每次请求的最大限制
        const int maxLimit = 1500;

        // 保存所有 K 线数据
        var allCandles = new List<QuoteCandleData>();

        // 当前的查询起始时间
        var currentStartTime = startTime;

        var interval = Converter.ConvertBarSize(barSize);

        while (currentStartTime < endTime)
        {
            // 计算当前批次的结束时间
            var currentEndTime = currentStartTime.AddSeconds((int)interval * maxLimit);
            if (currentEndTime > endTime)
            {
                currentEndTime = endTime;
            }

            // 查询当前时间范围的 K 线数据
            var result = m_RestClient.UsdFuturesApi.ExchangeData.GetKlinesAsync(
                symbol,
                interval,
                currentStartTime,
                currentEndTime,
                maxLimit
            ).Result;

            if (!result.Success)
            {
                throw new Exception($"Failed to query kline data for Symbol = \"{symbol}\" , barSize = \"{barSize}\", reason: {result.Error?.Message}");
            }

            // 将当前批次的 K 线数据添加到总集合中
            allCandles.AddRange(result.Data.Select(kline => Converter.ConvertQuoteCandleData(kline)));

            // 更新起始时间为当前批次的最后一条 K 线的时间
            currentStartTime = result.Data.Last().CloseTime.AddMilliseconds(1); // 避免重复数据
        }

        return allCandles;
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
