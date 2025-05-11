namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.ComponentModel;

[Component]
public class MarketMonitorService:ILifecycle,IMarketAnomalyNotifier, IMarketAnomalyConditionResultProvider
{
    private List<IMarketAnomalyMonitor> m_MarketAnomalyMonitors = new List<IMarketAnomalyMonitor>();

    private List<ISymbolTradeMonitor> m_SymbolTradeMonitors = new List<ISymbolTradeMonitor>();

    private IMarketAnomalyNotifier.MarketAnomalyListenerDelegate m_MarketAnomalyListeners;

    private class MarketAnomalyConditionResult
    {
        // 条件是否满足
        public bool Satisfied = false;

        // 异动值  
        public decimal Value = 0m;
    }

    private Dictionary<Type, MarketAnomalyConditionResult> m_MarketAnomalyConditionResultMap = new Dictionary<Type, MarketAnomalyConditionResult>();

    public void AddMarketAnomalyListener(IMarketAnomalyNotifier.MarketAnomalyListenerDelegate function)
    {
        m_MarketAnomalyListeners -= function;
        m_MarketAnomalyListeners += function;
    }

    public void RemoveMarketAnomalyListener(IMarketAnomalyNotifier.MarketAnomalyListenerDelegate function)
    {
        m_MarketAnomalyListeners -= function;
    }

    public void BrocastMarketAnomaly(string symbol, MarketAnomalyType type, decimal value, string message, MarketAnomalyColor color)
    {
        if(m_MarketAnomalyListeners != null)
        {
            m_MarketAnomalyListeners(symbol, type, value, message, color);
        }
    }

    public decimal GetValue<T>() where T : IDummyMarketAnomalyCondition
    {
        return GetValue(typeof(T));
    }

    public decimal GetValue(Type conditionType)
    {
        if(!m_MarketAnomalyConditionResultMap.ContainsKey(conditionType))
        {
            return decimal.MinValue;
        }

        return m_MarketAnomalyConditionResultMap[conditionType].Value;
    }

    public bool IsSatisfied<T>() where T : IDummyMarketAnomalyCondition
    {
        return IsSatisfied(typeof(T));
    }

    public bool IsSatisfied(Type conditionType)
    {
        if (!m_MarketAnomalyConditionResultMap.ContainsKey(conditionType))
        {
            return false;
        }

        return m_MarketAnomalyConditionResultMap[conditionType].Satisfied;
    }

    public override void OnStart()
    {
        
    }
}
