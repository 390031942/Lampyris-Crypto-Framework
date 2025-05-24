namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

/// <summary>
/// 提供行情数据的更新服务，该类主要接受并负责处理API获得的行情数据，引起回调事件，并存储到数据库
/// 该类消除了API行情数据的差异性，从而实现了行情数据结构的统一
/// </summary>
[Component]
public abstract class AbstractQuoteProviderService:ILifecycle
{
    [Autowired]
    protected QuoteCacheService m_CacheService;

    private QuoteDBIntegrityData m_QuoteDBIntegrityData;

    private const string QuoteDBIntegrityDataJsonPath = "quote/db_integrity.json";

    public override int Priority => 2;

    /// <summary>
    /// 存储全体USDT永续合约 symbol
    /// </summary>
    protected HashSet<string> m_Symbols = new HashSet<string>();

    /// <summary>
    /// 需要监听与更新的k线时间周期列表
    /// </summary>
    protected readonly BarSize[] m_ConcernedBarSizeList = new BarSize[3]
    {
        BarSize._1m,
        BarSize._15m,
        BarSize._1D,
    };

    /// <summary>
    /// Ticker更新事件
    /// </summary>
    /// <param name="dataList"></param>
    public delegate void OnTickerUpdateHandler(IEnumerable<QuoteTickerData> dataList);
    public OnTickerUpdateHandler OnTickerUpdated;

    /// <summary>
    /// K线更新事件
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    /// <param name="isEnd"></param>
    public delegate void OnCandleDataUpdateHandler(string symbol, BarSize barSize, QuoteCandleData candleData, bool isEnd);
    public OnCandleDataUpdateHandler OnCandleDataUpdated;

    public override void OnStart()
    {
        Logger.LogInfo("Start to initialize Quote Provider Service, " +
                       "integrity of candlestick data will be checked firstly, " +
                       "and then API-Level quote subscriptions will be made.");

        // 首先需要获取所有symbol的交易规则，对于交易规则，我们需要利用到每个symbol的上架时间
        APIUpdateUsdtFuturesSymbolsImpl();

        if (m_Symbols.Count == 0)
        {
            throw new InvalidDataException("Failed to verify market data: Symbol list is empty");
        }

        string? dirPath = Path.GetDirectoryName(QuoteDBIntegrityDataJsonPath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        if (File.Exists(QuoteDBIntegrityDataJsonPath))
        {
            string jsonContent = File.ReadAllText(QuoteDBIntegrityDataJsonPath);
            m_QuoteDBIntegrityData = JsonConvert.DeserializeObject<QuoteDBIntegrityData>(jsonContent);
        }
        else
        {
            m_QuoteDBIntegrityData = new QuoteDBIntegrityData();
        }

        // 验证k线数据完整性并订阅k线实时数据
        // 完整性指的是: 对于不同BarSize，如果这个barSize的数据是需要缓存的，则要确保缓存的数据完整
        Task.WaitAll(VerifyCandleDataIntegrityAndSubscription());
        File.WriteAllText(QuoteDBIntegrityDataJsonPath, JsonConvert.SerializeObject(m_QuoteDBIntegrityData));

        // API订阅ticker, 标记价格，trade数据等...
        // Task.WaitAll(APISubscriptionAllImpl());
    }

    #region Ticker行情

    /// <summary>
    /// Ticker行情字典(不需要存到数据库中，所以直接存储到这里)
    /// </summary>
    protected Dictionary<string, QuoteTickerData> m_QuoteTickerDataMap = new();

    /// <summary>
    /// Ticker行情列表(方便遍历)
    /// </summary>
    protected List<QuoteTickerData> m_QuoteTickerDataList = new();

    public IReadOnlyCollection<QuoteTickerData> GetTickerDataList()
    {
        return m_QuoteTickerDataList.AsReadOnly();
    }

    public QuoteTickerData QueryTickerData(string symbol)
    {
        if (!m_QuoteTickerDataMap.ContainsKey(symbol))
            return null;

        return m_QuoteTickerDataMap[symbol];
    }

    protected void PostProcessTickerData()
    {
        m_MarketSummaryData.Reset();

        decimal percentageSum = 0.0m;
        decimal top10PercentageSum = 0.0m;
        decimal last10PercentageSum = 0.0m;
        decimal mainStreamPercentageSum = 0.0m;

        m_QuoteTickerDataList.Sort((lhs, rhs) =>
        {
            if (lhs.ChangePerc == rhs.ChangePerc) return 0;
            return rhs.ChangePerc > lhs.ChangePerc ? 1 : -1;
        });

        for(int i = 0; i < m_QuoteTickerDataList.Count; i++)
        {
            var quoteTickerData = m_QuoteTickerDataList[i];
            if(quoteTickerData.ChangePerc > 0)
            {
                m_MarketSummaryData.RiseCount++;
            }
            else if(quoteTickerData.ChangePerc < 0)
            {
                m_MarketSummaryData.FallCount++;
            }
            else
            {
                m_MarketSummaryData.UnchangedCount++;
            }

            if (m_MainStreamSymbols.Contains(quoteTickerData.Symbol))
            {
                mainStreamPercentageSum += quoteTickerData.ChangePerc;
            }

            if(i < 10) 
            {
                top10PercentageSum += quoteTickerData.ChangePerc;
            }
            else if(i >= m_QuoteTickerDataList.Count - 10)
            {
                last10PercentageSum += quoteTickerData.ChangePerc;
            } 
            percentageSum += quoteTickerData.ChangePerc;

            var span = m_CacheService.QueryCacheOnlyLastestCandles(quoteTickerData.Symbol, BarSize._1m, 3);
            // 涨速计算
            if (span.Length > 0)
            { 
                quoteTickerData.RiseSpeed = 0m; 
            }
        }

        m_MarketSummaryData.AvgChangePerc = percentageSum / m_QuoteTickerDataList.Count;
        m_MarketSummaryData.MainStreamAvgChangePerc = mainStreamPercentageSum / m_QuoteTickerDataList.Count;
        m_MarketSummaryData.Top10AvgChangePerc = top10PercentageSum / m_QuoteTickerDataList.Count;
        m_MarketSummaryData.Last10AvgChangePerc = last10PercentageSum / m_QuoteTickerDataList.Count;
    }
    #endregion

    #region K线数据
    /// <summary>
    /// 查询指定时间范围内的k线列表
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="startTime">开始的时间范围</param>
    /// <param name="endTime">结束的时间范围</param>
    /// <param name="n">最多返回的k线数量，-1表示返回全部</param>
    /// <param name="cacheOnly">是否只从缓存中查询</param>
    /// <returns>返回符合条件的k线数据列表</returns>
    public ReadOnlySpan<QuoteCandleData> QueryCandleData(string symbol, BarSize barSize, DateTime? startTime = null, DateTime? endTime = null, int n = -1, bool cacheOnly = true)
    {
        // 如果只需要从缓存中查询，调用 QueryCacheOnlyCandleData
        if (cacheOnly)
        {
            return m_CacheService.QueryCacheOnlyCandleData(symbol, barSize, startTime, endTime, n);
        }

        // 从缓存和数据库中查询数据
        List<QuoteCandleData> result = m_CacheService.QueryCandleData(symbol, barSize, startTime, endTime, n);

        // 验证数据完整性
        if (!IsDataComplete(symbol, result, startTime, endTime, barSize))
        {
            // 如果数据不完整，调用 API 补充缺失的数据
            List<QuoteCandleData> apiData = APIQueryCandleDataImpl(symbol, barSize, startTime, endTime, n);

            // 合并 API 数据和缓存/数据库数据
            result = MergeCandleData(result, apiData, barSize);

            // 将补充的数据写入缓存（可选，根据业务需求）
            m_CacheService.StorageCandleData(symbol, barSize, apiData,true);
        }

        // 返回结果的 Span
        return CollectionsMarshal.AsSpan(result);
    }

    /// <summary>
    /// 合并缓存/数据库数据和API数据
    /// </summary>
    /// <param name="existingData">现有的k线数据</param>
    /// <param name="apiData">从API获取的k线数据</param>
    /// <param name="barSize">k线时间周期</param>
    /// <returns>合并后的k线数据</returns>
    private List<QuoteCandleData> MergeCandleData(List<QuoteCandleData> existingData, List<QuoteCandleData> apiData, BarSize barSize)
    {
        // 合并数据
        List<QuoteCandleData> mergedData = new List<QuoteCandleData>(existingData);
        mergedData.AddRange(apiData);

        // 去重并按时间排序
        mergedData = mergedData
            .GroupBy(c => c.DateTime)
            .Select(g => g.First())
            .OrderBy(c => c.DateTime)
            .ToList();

        return mergedData;
    }

    /// <summary>
    /// API查询-K线数据
    /// </summary>
    public abstract List<QuoteCandleData> APIQueryCandleDataImpl(string symbol, BarSize barSize, DateTime? startTime, DateTime? endTime, int n = -1);

    /// <summary>
    /// API订阅-k线数据
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    public abstract void APISubscribeCandleData(string symbol, BarSize barSize);
    #endregion

    #region 交易订阅
    #endregion

    #region 市场概况
    private MarketSummaryData m_MarketSummaryData = new MarketSummaryData();

    private HashSet<string> m_MainStreamSymbols = new HashSet<string>()
    {
        "BTCUSDT",
        "ETHUSDT"
    };
    #endregion

    #region 交易数据
    #endregion

    #region 深度数据
    #endregion

    #region API订阅
    /// <summary>
    /// 初始化API的行情订阅,这些API订阅将在程序运行时一直维持订阅的状态，不受到客户端订阅的影响
    /// 这里的行情订阅不包括K线，因为K线数据需要验证完整性
    /// </summary>
    protected abstract Task APISubscriptionAllImpl();

    /// <summary>
    /// 更新全体USDT永续合约列表，包括上架时间
    /// </summary>
    /// <returns></returns>
    protected abstract void APIUpdateUsdtFuturesSymbolsImpl();

    /// <summary>
    /// 获取全体USDT永续合约列表
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetSymbolList()
    {
        return m_Symbols;
    }
    #endregion

    #region Symbol增删

    // 处理删减的临时集合
    private HashSet<string> m_TempSymbolSet = new HashSet<string>();

    /// <summary>
    /// 动态处理新增或移除的 symbol
    /// </summary>
    protected void HandleSymbolChanges()
    {
        // 检查是否有移除的 symbol
        m_TempSymbolSet.Clear();
        m_TempSymbolSet.UnionWith(m_Symbols);

        APIUpdateUsdtFuturesSymbolsImpl();

        var currentSymbols = GetSymbolList();
        var increasedSymbols = currentSymbols.Except(m_Symbols);
        var removedSymbols = m_Symbols.Except(currentSymbols);

        foreach (var removedSymbol in removedSymbols)
        {
            m_Symbols.Remove(removedSymbol);
            APIUnsubscribeFromSymbolImpl(removedSymbol);
        }

        foreach (var increasedSymbol in increasedSymbols)
        {
            m_Symbols.Add(increasedSymbol);
            APISubscribeToSymbolImpl(increasedSymbol);
        }
    }

    /// <summary>
    /// 订阅单个 symbol 的所有数据
    /// </summary>
    protected abstract void APISubscribeToSymbolImpl(string symbol);

    /// <summary>
    /// 取消订阅单个 symbol 的所有数据
    /// </summary>
    protected abstract void APIUnsubscribeFromSymbolImpl(string symbol);
    #endregion

    /// <summary>
    /// 获取服务器时间(实现)
    /// </summary>
    protected abstract DateTime GetAPIServerDateTimeImpl();

    /// <summary>
    /// 本地与服务器时间之差
    /// </summary>
    protected TimeSpan? m_LocalAndServerTimeSpan;

    /// <summary>
    /// m_LocalAndServerTimeSpan线程锁
    /// </summary>
    private readonly object m_TimeSpanLock = new object();

    public DateTime GetAPIServerDateTime()
    {
        if (m_LocalAndServerTimeSpan == null)
        {
            lock (m_TimeSpanLock)
            {
                if (m_LocalAndServerTimeSpan == null)
                {
                    DateTime serverDateTime = GetAPIServerDateTimeImpl();
                    DateTime localTime = DateTime.UtcNow;
                    m_LocalAndServerTimeSpan = serverDateTime - localTime;
                    return serverDateTime;
                }
            }
        }

        return DateTime.UtcNow + m_LocalAndServerTimeSpan.Value;
    }

    /// <summary>
    /// 验证行情数据的完整性
    /// </summary>s
    protected async Task VerifyCandleDataIntegrityAndSubscription()
    {
        // 记录当前时间
        DateTime now = GetAPIServerDateTime();

        now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
        // 使用 Task 并行处理每个 symbol
        foreach (var symbol in m_Symbols)
        {
            VerifySymbol(symbol, now);
        }

        // var tasks = m_Symbols.Select(symbol => Task.Run(() => VerifySymbol(symbol, now)));
        // await Task.WhenAll(tasks);

        // 补充完缺失的时间段数据后，再次对每个symbol进行处理:
        // 补充下载 时间段为 [之前记录的当前时间now，当前时间]之间的数据，下载完毕后立刻开启数据的订阅。
        // 特别注意：如果当前时间的秒数大于55秒，则在下一分钟开始时候再进行操作
        // 这样做的目的是为了避免在：时刻dateTime1的时候请求，在dateTime2的时候完成，且dateTime1.minute != dateTime2.minute的时候
        // dateTime1.minute分钟的数据是不对的，因为请求完成后订阅的是dateTime2.minute开始的数据了，导致dateTime1.minute的数据不是最终的数据。
        // await SubscribeAndDownloadMissingData(now);
    }

    /// <summary>
    /// 检查时间区间的完整性，并返回缺失的时间区间列表
    /// </summary>
    /// <param name="dateTimeList">已存在的时间点列表</param>
    /// <param name="startTime">起始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="interval">时间间隔</param>
    /// <returns>缺失的时间区间列表</returns>
    private List<(DateTime, DateTime)> GetMissingIntervals(IEnumerable<DateTime> dateTimeList, DateTime startTime, DateTime endTime, TimeSpan interval)
    {
        List<(DateTime, DateTime)> missingIntervals = new List<(DateTime, DateTime)>();

        if(!dateTimeList.Any())
        {
            missingIntervals.Add((startTime, endTime));
            return missingIntervals;
        }
        // 确保时间点按升序排序
        var sortedDateTimeList = dateTimeList.OrderBy(dt => dt);

        // 检查缺失时间区间
        DateTime? previousTime = startTime; // 从起始时间开始检查
        foreach (var currentTime in sortedDateTimeList)
        {
            if (previousTime.HasValue && currentTime > previousTime.Value.Add(interval))
            {
                // 如果当前时间与前一个时间之间有缺失，记录缺失区间
                missingIntervals.Add((previousTime.Value.Add(interval), currentTime));
            }

            // 更新前一个时间
            previousTime = currentTime;
        }

        // 检查最后一个时间点到结束时间是否有缺失
        if (endTime > previousTime.Value.Add(interval))
        {
            missingIntervals.Add((previousTime.Value.Add(interval), endTime));
        }

        return missingIntervals;
    }

    /// <summary>
    /// 验证数据的完整性
    /// </summary>
    /// <param name="data">现有的k线数据</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="barSize">k线时间周期</param>
    /// <returns>数据是否完整</returns>
    private bool IsDataComplete(string symbol, List<QuoteCandleData> data, DateTime? startTime, DateTime? endTime, BarSize barSize)
    {
        if (data == null || data.Count == 0)
        {
            return false;
        }

        // 提取现有的时间点列表
        IEnumerable<DateTime> dateTimeList = data.Select(c => c.DateTime);

        // 获取时间间隔
        TimeSpan interval = DateTimeExtensions.GetInterval(barSize);

        if(!startTime.HasValue)
        {
            if(m_Symbol2TradeRuleMap.TryGetValue(symbol, out var tradeRule))
            {
                startTime = DateTimeUtil.FromUnixTimestamp(tradeRule.OnBoardTimestamp);
            }
        }

        if(!endTime.HasValue)
        {
            endTime = GetAPIServerDateTime();
        }

        // 检查缺失的时间区间
        var missingIntervals = GetMissingIntervals(dateTimeList, startTime.Value, endTime.Value, interval);

        // 如果没有缺失区间，则数据完整
        return missingIntervals.Count == 0;
    }

    /// 处理单个 symbol 的数据验证和补充, 以确保"缓存数据中最早的时刻"到now时刻之间的数据完整
    /// 其中"缓存数据中最早的时刻"，是由barSize决定的。
    /// 针对不同的barSize有不同的缓存策略(参考QuoteCandleDataCacheStrategy类)
    /// "缓存数据中最早的时刻"根据缓存策略所需的数据时长计算得到
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="now">当前时刻</param>
    private void VerifySymbol(string symbol,DateTime now)
    {
        // 获取该 symbol 的上线时间
        if(!m_Symbol2TradeRuleMap.TryGetValue(symbol, out var tradeRule))
        {
            Logger.LogError($"Failed to verify symbol \"{symbol}\", because it's trade rule doesn't represent in m_Symbol2TradeRuleMap");
            return;
        }

        Logger.LogInfo($"[{Thread.CurrentThread.ManagedThreadId}]Start to verify symbol \"{symbol}\"");

        DateTime onBoardDateTime = DateTimeUtil.FromUnixTimestamp(tradeRule.OnBoardTimestamp);

        foreach (var strategy in m_CacheService.GetCacheStrategies())
        {
            var barSize = strategy.BarSize;

            DateTime? cacheStartDateTime = null; //  strategy.CalculateCacheStartDateTime(now);

            if(cacheStartDateTime == null)
            {
                cacheStartDateTime = onBoardDateTime;
            }
            else
            {
                // 缓存的开始时间不能比上架时间还早
                if (cacheStartDateTime < onBoardDateTime)
                {
                    cacheStartDateTime = onBoardDateTime;
                }
            }

            // 获取已存在的时间点列表
            List<DateTime> dateTimeList = m_CacheService.QueryCandleDateTimeList(symbol, barSize).ToList();

            // 获取时间间隔
            TimeSpan interval = DateTimeExtensions.GetInterval(barSize);

            // 检查缺失时间区间
            var missingIntervals = GetMissingIntervals(dateTimeList, cacheStartDateTime.Value, now, interval);
            Logger.LogInfo($"[{Thread.CurrentThread.ManagedThreadId}]Symbol \"{symbol}\",{barSize} missingIntervalsCount =  {missingIntervals.Count}");

            if (!m_QuoteDBIntegrityData.SymbolIntegrityDataMap.ContainsKey(symbol))
            {
                m_QuoteDBIntegrityData.SymbolIntegrityDataMap[symbol] = new QuoteDBIntegrityData.PerSymbolIntegrityData();
            }

            QuoteDBIntegrityData.PerSymbolIntegrityData integrityData = m_QuoteDBIntegrityData.SymbolIntegrityDataMap[symbol];
            
            // 对于每一个缺失的时间区间，进行下载并存储
            foreach (var missingInterval in missingIntervals)
            {
                if (missingInterval.Item1 >= integrityData.StartDate && missingInterval.Item2 <= integrityData.EndDate)
                    continue;

                Logger.LogInfo($"[{Thread.CurrentThread.ManagedThreadId}]Begin to uery to symbol \"{symbol}\", interval = ({missingInterval.Item1.ToString("yyyy-MM-dd)")}" +
                 $"{missingInterval.Item2.ToString("yyyy-MM-dd)")}");

                var result = APIQueryCandleDataImpl(symbol, barSize, missingInterval.Item1, missingInterval.Item2);

                Logger.LogInfo($"[{Thread.CurrentThread.ManagedThreadId}]Finished to query to symbol \"{symbol}\", interval = ({missingInterval.Item1.ToString("yyyy-MM-dd)")}" +
                 $"{missingInterval.Item2.ToString("yyyy-MM-dd)")}");
                // m_CacheService.StorageCandleData(symbol, barSize, result);
            }

            integrityData.StartDate = cacheStartDateTime.Value;
            integrityData.EndDate   = now;
        }
    }

    /// <summary>
    /// 补充下载 [之前记录的当前时间 now，当前时间] 之间的数据，并订阅
    /// </summary>
    private async Task SubscribeAndDownloadMissingData(DateTime previousNow)
    {
        // 使用 Task 并行处理每个 symbol
        var tasks = m_Symbols.Select(symbol => Task.Run(() =>
        {
            foreach (BarSize barSize in m_ConcernedBarSizeList)
            {
                var now = GetAPIServerDateTime();
                if (now.Second > 55)
                {
                    Task.Delay((60 - now.Second) * 1000).Wait();
                    now = GetAPIServerDateTime(); // 再次获取时间
                }
                // 下载 [previousNow, now] 之间的数据
                var result = APIQueryCandleDataImpl(symbol, barSize, previousNow, now);
                m_CacheService.StorageCandleData(symbol, barSize, result);

                // 开启数据订阅
                APISubscribeCandleData(symbol, barSize);
            }
        }));

        await Task.WhenAll(tasks);
    }

    #region 交易规则

    protected Dictionary<string, SymbolTradeRule> m_Symbol2TradeRuleMap = new Dictionary<string, SymbolTradeRule>();

    public IEnumerable<SymbolTradeRule> QueryAllTradeRule()
    {
        return m_Symbol2TradeRuleMap.Select(kvp => kvp.Value);
    }

    public SymbolTradeRule QueryTradeRule(string symbol)
    {
        return m_Symbol2TradeRuleMap.ContainsKey(symbol) ? m_Symbol2TradeRuleMap[symbol] : null;
    }

    public IEnumerable<SymbolTradeRule> QueryTradeRuleBySymbolList(IEnumerable<string> symbolList)
    {
        foreach(string symbol in symbolList)
        {
            yield return QueryTradeRule(symbol);
        }
    }
    #endregion
}