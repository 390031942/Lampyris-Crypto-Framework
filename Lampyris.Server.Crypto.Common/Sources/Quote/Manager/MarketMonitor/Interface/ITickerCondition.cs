namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  对Ticker数据 监视条件 接口类
/// </summary>
public abstract class ITickerCondition
{
    public abstract bool Test(QuoteTickerData quoteTickerData);
}