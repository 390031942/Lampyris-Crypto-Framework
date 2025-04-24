namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System;

public interface IIndicatorCalculator<T>
{
    /// <summary>
    /// 计算指标并存储到缓存中
    /// </summary>
    /// <param name="prices">传入-价格列表</param>
    /// <param name="indicateValues">传出-指标值列表</param>
    void CalculateAndStore(CircularQueue<decimal> prices, CircularQueue<T> indicateValues, bool append);
}