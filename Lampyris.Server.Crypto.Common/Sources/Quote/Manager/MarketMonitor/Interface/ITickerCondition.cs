namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  对Ticker数据 监视条件 接口类
/// </summary>
public abstract class ITickerCondition:IDummyMarketAnomalyCondition
{
    /// <summary>
    /// 测试Ticker数据是否满足条件
    /// </summary>
    /// <param name="quoteTickerData">ticker数据</param>
    /// <param name="value">异动数值</param>
    /// <returns></returns>
    public abstract bool Test(QuoteTickerData quoteTickerData, out decimal value);
}