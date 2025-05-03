namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.ComponentModel;

public abstract class ISymbolCoolDownChecker
{
    /// <summary>
    /// 冷却时间（毫秒）
    /// </summary>
    public abstract int CoolDownTime { get; }

    /// <summary>
    /// 上次触发的时间戳
    /// </summary>
    protected Dictionary<string, DateTime> LastTriggeredTimeMap = new Dictionary<string, DateTime>();

    public bool CheckCD(string symbol)
    {
        var LastTriggeredTime = LastTriggeredTimeMap.GetValueOrDefault(symbol);
        if(LastTriggeredTime == DateTime.MinValue || (DateTime.UtcNow - LastTriggeredTime).TotalMilliseconds <= CoolDownTime)
        {
            LastTriggeredTime = DateTime.UtcNow;
            return true;
        }
        return false;
    }
}

public abstract class ITickerCondition
{
    public abstract bool Test(QuoteTickerData quoteTickerData);
}

public abstract class ICandleCondition
{
    public BarSize BarSize { get; protected set; }
    public abstract int ExpectedCount { get; }

    public ICandleCondition(BarSize barSize)
    {
        BarSize = barSize;
    }
}

/// <summary>
/// k线成交额条件
/// </summary>
public class CandleCurrencyCondition : ICandleCondition
{
    public decimal Threshold { get; private set; }
    public override int ExpectedCount => 1;

    public CandleCurrencyCondition(BarSize barSize, decimal threshold) : base(barSize)
    {
        Threshold = threshold;
    }

    public bool Test(Span<QuoteCandleData> dataList)
    {
        return dataList[dataList.Length - 1].Currency >= (double)Threshold;
    }
}

/// <summary>
/// 涨速条件
/// </summary>
public class RiseSpeedCondition : ITickerCondition
{
    public decimal Threshold { get; private set; }
    public bool Greater { get; private set; }

    public RiseSpeedCondition(decimal threshold, bool greater)
    {
        Threshold = threshold;
        Greater = greater;
    }

    public override bool Test(QuoteTickerData quoteTickerData)
    {
        if (quoteTickerData == null)
        {
            return false;
        }


        return Greater ?
            quoteTickerData.RiseSpeed >= Threshold :
            quoteTickerData.RiseSpeed <= Threshold;
    }
}

/// <summary>
/// k线连续红/绿条件
/// </summary>
public class CandleContinuousColorCondition : ICandleCondition
{
    public int Threshold { get; private set; }

    public bool IsRise { get; private set; }

    public override int ExpectedCount => Threshold;

    public CandleContinuousColorCondition(BarSize barSize, bool isRise, int threshold) : base(barSize)
    {
        Threshold = threshold;
        IsRise = isRise;
    }

    public bool Test(Span<QuoteCandleData> dataList)
    {
        foreach(QuoteCandleData data in dataList)
        {
            if(!(data.ChangePerc > 0 && IsRise) && !(data.ChangePerc < 0 && !IsRise))
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// 区间放量条件: IntervalLength1内的平均成交量 * 倍数Times < IntervalLength2分内的平均成交量 
/// </summary>
public class IntervalVolumeSurgeCondition : ICandleCondition
{
    public IntervalVolumeSurgeCondition(BarSize barSize, int interval1, int interval2, int times) : base(barSize)
    {
        IntervalLength1 = interval1;
        IntervalLength2 = interval2;
        Times = times;
    }

    public int IntervalLength1 { get; private set; }
    public int IntervalLength2 { get; private set; }

    public int Times { get; private set; }

    public override int ExpectedCount => IntervalLength1;

    public bool Test(Span<QuoteCandleData> dataList)
    {
        // IntervalLength1分钟内的平均成交额
        double avg1 = 0;

        // IntervalLength2分钟内的平均成交额
        double avg2 = 0;

        int index = 0;
        foreach (QuoteCandleData data in dataList)
        {
            avg1 += data.Currency;

            if (index >= ExpectedCount - IntervalLength2)
            {
                avg2 += data.Currency;
            }
            index++;
        }

        return avg1 * IntervalLength2 * Times < avg2 * IntervalLength1;
    }
}

/// <summary>
/// 成交异常 监视器
/// </summary>
public abstract class ISymbolTradeMonitor
{
    public abstract void Update(QuoteTradeData quoteTradeData);
}


/// <summary>
/// 成交数据流异常 监视器
/// </summary>
public class TradeSurgeMonitor:ISymbolTradeMonitor 
{
    public class PerSecondTradeData
    {
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Currency => Price * Quantity;

        /// <summary>
        /// 交易笔数
        /// </summary>
        public int TradeCount { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        public void Append(QuoteTradeData quoteTradeData)
        {
            Price += quoteTradeData.Price;
            Quantity += quoteTradeData.Quantity;
            TradeCount += quoteTradeData.TradeCount;
        }
    }

    /// <summary>
    /// 缓存的秒级成交数据
    /// </summary>
    private int m_PerSecondTradeDataStorageCount;

    /// <summary>
    /// 增量维护总成交额
    /// </summary>
    private decimal m_TotalTradeCurrency;

    /// <summary>
    /// 当前秒 持续的毫秒数
    /// </summary>
    private int m_LastingMs { get; set; }

    /// <summary>
    /// 近若干秒的 每秒数据
    /// </summary>
    private CircularQueue<PerSecondTradeData> m_PerSecondTradeDataList;

    public TradeSurgeMonitor(int perSecondTradeDataStorageCount = 300)
    {
        m_PerSecondTradeDataStorageCount = perSecondTradeDataStorageCount;
        m_PerSecondTradeDataList = new(perSecondTradeDataStorageCount);
    }

    public override void Update(QuoteTradeData quoteTradeData)
    {
        // 如果列表为空 或者 秒数切换了，就追加一条
        if (m_PerSecondTradeDataList.Count == 0 ||
            (m_PerSecondTradeDataList.Count > 0 &&
            DateTimeUtil.SecondEqual(m_PerSecondTradeDataList.Last().Time, quoteTradeData.TradeTime)))
        {
            // 如果队列已满，移除最早的一条数据，并从总成交额中减去它的金额
            if (m_PerSecondTradeDataList.Count == m_PerSecondTradeDataStorageCount)
            {
                var oldestData = m_PerSecondTradeDataList.First();
                m_TotalTradeCurrency -= oldestData.Currency;
            }

            var perSecondTradeData = m_PerSecondTradeDataList.Count == m_PerSecondTradeDataStorageCount ? m_PerSecondTradeDataList.First() : new PerSecondTradeData()
            {
                Time = new DateTime(quoteTradeData.TradeTime.Year, quoteTradeData.TradeTime.Month, quoteTradeData.TradeTime.Day,
                                    quoteTradeData.TradeTime.Hour, quoteTradeData.TradeTime.Minute, quoteTradeData.TradeTime.Second)
            };

            m_PerSecondTradeDataList.Enqueue(perSecondTradeData);
        }

        // 把当前秒的数据追加进去
        var last = m_PerSecondTradeDataList.Last();
        last.Append(quoteTradeData);
        m_TotalTradeCurrency += quoteTradeData.Currency;
        m_LastingMs = quoteTradeData.TradeTime.Microsecond;
    }

    /// <summary>
    /// 计算近若干秒的平均成交额
    /// </summary>
    /// <returns>平均每秒成交额</returns>
    private decimal CalculateAverageTradeAmount()
    {
        if (m_PerSecondTradeDataList.Count == 0)
            return 0;

        // 直接通过总成交额计算平均值
        return m_TotalTradeCurrency / m_PerSecondTradeDataList.Count;
    }

    /// <summary>
    /// 测试当前秒的预估成交额是否大于近300秒的每秒平均成交额的100倍
    /// </summary>
    /// <param name="currentTradeData">当前秒的成交数据</param>
    /// <returns>是否异常</returns>
    public bool Test(PerSecondTradeData currentTradeData)
    {
        // 计算近若干秒的平均成交额
        decimal averageTradeAmount = CalculateAverageTradeAmount();

        // 判断是否大于100倍
        return averageTradeAmount * 100 < currentTradeData.Currency;
    }
}

/// <summary>
/// 市场异动监控策略基类 
/// </summary>
public abstract class IMarketAnomalyMonitor : ISymbolCoolDownChecker
{
    protected abstract bool CheckImpl();

    public bool Check()
    {
        return CheckCD() ? CheckImpl():false;
    } 
}

public class CandleContinuousColorMonitor: IMarketAnomalyMonitor
{
    public override int CoolDownTime => 30 * 60 * 100; 
    public CandleContinuousColorMonitor()
    {
        CandleContinuousColorCondition condition1 = new CandleContinuousColorCondition(BarSize._1m, true, 5);
        CandleCurrencyCondition condition2 = new CandleCurrencyCondition(BarSize._1m, 100000);

        condition1.ExpectedCount;
        condition2.ExpectedCount;
    }
}

[Component]
public class MarketMonitorService:ILifecycle
{
    [Autowired]
    private AbstractQuoteProviderService m_QuoteProviderService;

    public override void OnStart()
    {
        m_QuoteProviderService.OnTickerUpdated += TestTickerData;
        m_QuoteProviderService.OnCandleDataUpdated += TestCandleData;
    }

    public void TestTickerData(IEnumerable<QuoteTickerData> dataList)
    {

    }

    public void TestCandleData(string symbol, BarSize barSize, bool isEnd)
    {

    }
}
