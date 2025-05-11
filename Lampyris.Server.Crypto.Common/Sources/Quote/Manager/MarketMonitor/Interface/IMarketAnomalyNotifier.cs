namespace Lampyris.Server.Crypto.Common;

public interface IMarketAnomalyNotifier
{
    /// <summary>
    /// 市场异动监控事件
    /// </summary>
    /// <param name="symbol">符号对</param>
    /// <param name="type">异动类型</param>
    /// <param name="value">异动的数值</param>
    /// <param name="message">异动消息，用于消息</param>
    /// <param name="color">异动消息颜色</param>
    public delegate void MarketAnomalyListenerDelegate(string symbol, MarketAnomalyType type, decimal value, string message, MarketAnomalyColor color);

    public void AddMarketAnomalyListener(MarketAnomalyListenerDelegate function);

    public void RemoveMarketAnomalyListener(MarketAnomalyListenerDelegate function);

    public void BrocastMarketAnomaly(string symbol, MarketAnomalyType type, decimal value, string message, MarketAnomalyColor color);
}
