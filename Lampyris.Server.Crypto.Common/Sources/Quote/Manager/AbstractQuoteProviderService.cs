namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.Collections.Generic;

/// <summary>
/// 提供行情数据的更新服务，该类主要接受并负责处理API获得的行情数据，引起回调事件，并存储到数据库
/// 该类消除了API行情数据的差异性，从而实现了行情数据结构的统一
/// </summary>
[Component]
public abstract class AbstractQuoteProviderService:ILifecycle
{
    [Autowired]
    protected QuoteCacheService m_CacheService;

    public override int Priority => 2;

    /// <summary>
    /// 存储全体USDT永续合约 symbol
    /// </summary>
    protected HashSet<string> m_Symbols = new HashSet<string>();

    /// <summary>
    /// symbol -> 上架时间
    /// </summary>
    protected Dictionary<string, DateTime> m_Symbol2OnBoardTime = new Dictionary<string, DateTime>();

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
    public delegate void OnCandleDataUpdateHandler(string symbol, BarSize barSize, bool isEnd);
    public OnCandleDataUpdateHandler OnCandleDataUpdated;

    public override void OnStart()
    {
        Logger.LogInfo("Start to initialize Quote Provider Service, " +
                       "integrity of candlestick data will be checked firstly, " +
                       "and then API-Level quote subscriptions will be made.");

        Task[] taskList = new Task[] {
            // API订阅ticker, 标记价格，trade数据等...
            APISubscriptionAllImpl(),

            // 验证k线数据完整性并订阅k线实时数据
            VerifyCandleDataIntegrityAndSubscription()
        };
        Task.WaitAll(taskList);
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
    #endregion

    #region K线查询
    #endregion

    #region K线数据
    /// <summary>
    /// API查询-K线数据
    /// </summary>
    public abstract List<QuoteCandleData> APIQueryCandleDataImpl(string symbol, BarSize barSize, DateTime startTime, DateTime endTime);

    /// <summary>
    /// API订阅-k线数据
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    public abstract void APISubscribeCandleData(string symbol, BarSize barSize);
    #endregion

    #region 市场概况
    #endregion

    #region 交易订阅
    #endregion

    #region 市场概况
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
    /// 更新全体USDT永续合约列表，包括上架时间
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetSymbolList()
    {
        return m_Symbols;
    }
    #endregion

    #region Symbol增删
    /// <summary>
    /// 动态处理新增或移除的 symbol
    /// </summary>
    protected void HandleSymbolChanges()
    {
        // 检查是否有移除的 symbol
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
    /// </summary>
    protected async Task VerifyCandleDataIntegrityAndSubscription()
    {
        // 首先需要获取所有symbol的交易规则，对于交易规则，我们需要利用到每个symbol的上架时间
        APIUpdateUsdtFuturesSymbolsImpl();

        if (m_Symbols.Count == 0)
        {
            throw new InvalidDataException("Failed to verify market data: Symbol list is empty");
        }

        // 记录当前时间
        DateTime now = GetAPIServerDateTime();

        // 使用 Task 并行处理每个 symbol
        var tasks = m_Symbols.Select(symbol => Task.Run(() => ValifySymbol(symbol, now)));
        await Task.WhenAll(tasks);

        // 补充完缺失的时间段数据后，再次对每个symbol进行处理:
        // 补充下载 时间段为 [之前记录的当前时间now，当前时间]之间的数据，下载完毕后立刻开启数据的订阅。
        // 特别注意：如果当前时间的秒数大于55秒，则在下一分钟开始时候再进行操作
        // 这样做的目的是为了避免在：时刻dateTime1的时候请求，在dateTime2的时候完成，且dateTime1.minute != dateTime2.minute的时候
        // dateTime1.minute分钟的数据是不对的，因为请求完成后订阅的是dateTime2.minute开始的数据了，导致dateTime1.minute的数据不是最终的数据。
        await SubscribeAndDownloadMissingData(now);
    }

    /// <summary>
    /// 处理单个 symbol 的数据验证和补充
    /// </summary>
    private void ValifySymbol(string symbol, DateTime now)
    {
        List<(DateTime, DateTime)> missingIntervalList = new List<(DateTime, DateTime)>();

        // 获取该 symbol 的上线时间
        m_Symbol2OnBoardTime.TryGetValue(symbol, out var onboardDateTime);

        foreach (BarSize barSize in m_ConcernedBarSizeList)
        {
            // 获取已存在的时间点列表
            IEnumerable<DateTime> dateTimeList = m_CacheService.QueryCandleDateTimeList(symbol, barSize);

            // 确保时间点按升序排序
            var sortedDateTimeList = dateTimeList.OrderBy(dt => dt);

            // 获取时间间隔
            TimeSpan interval = DateTimeExtensions.GetInterval(barSize);

            // 检查缺失时间区间
            DateTime? previousTime = onboardDateTime; // 从上线时间开始检查
            foreach (var currentTime in sortedDateTimeList)
            {
                if (previousTime.HasValue && currentTime > previousTime.Value.Add(interval))
                {
                    // 如果当前时间与前一个时间之间有缺失，记录缺失区间
                    missingIntervalList.Add((previousTime.Value.Add(interval), currentTime));
                }

                // 更新前一个时间
                previousTime = currentTime;
            }

            // 检查最后一个时间点到当前时间是否有缺失
            if (previousTime.HasValue && now > previousTime.Value.Add(interval))
            {
                missingIntervalList.Add((previousTime.Value.Add(interval), now));
            }

            // 对于每一个缺失的时间区间，进行下载并存储
            foreach (var missingInterval in missingIntervalList)
            {
                var result = APIQueryCandleDataImpl(symbol, barSize, missingInterval.Item1, missingInterval.Item2);
                m_CacheService.StorageCandleData(symbol, barSize, result);
            }
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
 }