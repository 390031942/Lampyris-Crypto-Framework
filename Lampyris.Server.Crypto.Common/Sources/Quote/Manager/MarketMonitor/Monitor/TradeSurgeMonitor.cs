namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

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
