namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 对逐笔成交数据 监视条件 接口类
/// </summary>
public abstract class ISymbolTradeMonitor
{
    public abstract void Update(QuoteTradeData quoteTradeData);
}
