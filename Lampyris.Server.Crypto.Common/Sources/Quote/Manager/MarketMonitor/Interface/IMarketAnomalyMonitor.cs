namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 市场异动监控策略基类 
/// </summary>
public abstract class IMarketAnomalyMonitor : ISymbolCoolDownChecker
{
    private List<ICandleCondition> m_CandleConditionList;
    private List<ITickerCondition> m_TickerConditionList;

    protected QuoteCacheService m_QuoteCacheService;

    public bool Check(string symbol)
    {
        if (CheckCD(symbol))
        {
            foreach (var condition in m_CandleConditionList)
            {
                int count = condition.ExpectedCount;
                m_QuoteCacheService.QueryLastestCandles(symbol, condition.BarSize, count);
            }
            MarkCD(symbol);
        }

        return false;
    } 
}
