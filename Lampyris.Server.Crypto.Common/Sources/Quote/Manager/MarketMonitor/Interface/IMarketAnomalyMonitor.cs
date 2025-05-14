using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 市场异动监控策略基类 
/// </summary>
public abstract class IMarketAnomalyMonitor : ISymbolCoolDownChecker
{
    private List<ICandleCondition> m_CandleConditionList;
    private List<ITickerCondition> m_TickerConditionList;

    protected QuoteCacheService m_QuoteCacheService;
    protected AbstractQuoteProviderService m_AbstractQuoteProviderService;

    /// <summary>
    /// Symbol -> (Ticker条件满足,Candle条件满足)
    /// </summary>
    protected Dictionary<string, ValueTuple<bool, bool>> m_SatisfactionMap = new Dictionary<string, ValueTuple<bool, bool>>();

    protected void AddCondition(ICandleCondition candleCondition)
    {
        m_CandleConditionList.Add(candleCondition);
    }
    protected void AddCondition(ITickerCondition tickerCondition)
    {
        m_TickerConditionList.Add(tickerCondition);
    }

    protected IMarketAnomalyMonitor()
    {
        m_QuoteCacheService = Components.GetComponent<QuoteCacheService>();
        m_AbstractQuoteProviderService = Components.GetComponent<AbstractQuoteProviderService>();

        m_AbstractQuoteProviderService.OnTickerUpdated += this.OnTickerUpdate;
        m_AbstractQuoteProviderService.OnCandleDataUpdated += this.OnCandleDataUpdate;
    }

    private void OnCandleDataUpdate(string symbol, BarSize barSize, QuoteCandleData data, bool isEnd)
    {
        if (CheckCD(symbol))
        {
            foreach (var condition in m_CandleConditionList)
            {
                if (barSize.Equals(condition.BarSize))
                {
                    if(!condition.Test(m_QuoteCacheService.QueryCacheOnlyLastestCandles(symbol, barSize, condition.ExpectedCount), isEnd, out var a))
                    {
                        return;
                    }
                }
            }

            var tuple = m_SatisfactionMap[symbol];
            tuple.Item2 = false;

            if (tuple.Item1 && tuple.Item2)
            {
                tuple.Item1 = false;
                tuple.Item2 = false;

                MarkCD(symbol);
            }
        }
    }

    private void OnTickerUpdate(IEnumerable<QuoteTickerData> dataList)
    {
        foreach (var data in dataList)
        {
            if (CheckCD(data.Symbol))
            {
                if (!m_SatisfactionMap.ContainsKey(data.Symbol))
                {
                    m_SatisfactionMap[data.Symbol] = new ValueTuple<bool, bool>(false, false);
                }
                bool satisfield = true;
                foreach (var condition in m_TickerConditionList)
                {
                    if (!condition.Test(data, out var a ))
                    {
                        satisfield = false;
                        break;
                    }
                }

                if(satisfield)
                {
                    var tuple = m_SatisfactionMap[data.Symbol];
                    tuple.Item1 = true;

                    if (tuple.Item1 && tuple.Item2)
                    {
                        tuple.Item1 = false;
                        tuple.Item2 = false;

                        MarkCD(data.Symbol);
                    }
                }
            }
        }
    }
}
