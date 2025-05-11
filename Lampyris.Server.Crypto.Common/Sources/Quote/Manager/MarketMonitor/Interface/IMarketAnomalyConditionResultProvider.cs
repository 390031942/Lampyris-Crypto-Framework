namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 供Monitor使用，用于查询ITickerCondition和ICandleCondition的结果
/// </summary>
public interface IMarketAnomalyConditionResultProvider
{
    /// <summary>
    /// 条件是否满足
    /// </summary>
    /// <typeparam name="T">异动条件类型</typeparam>
    /// <returns></returns>
    public bool IsSatisfied<T>() where T : IDummyMarketAnomalyCondition;

    /// <summary>
    /// 异动的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public decimal GetValue<T>() where T : IDummyMarketAnomalyCondition;

    /// <summary>
    /// 条件是否满足
    /// </summary>
    /// <param name="conditionType">异动条件类型</param>
    /// <returns></returns>
    public bool IsSatisfied(Type conditionType);

    /// <summary>
    /// 异动的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="conditionType">异动条件类型</param>
    /// <returns></returns>
    public decimal GetValue(Type conditionType);
}
