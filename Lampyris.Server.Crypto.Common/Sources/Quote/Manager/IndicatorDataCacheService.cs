namespace Lampyris.Server.Crypto.Common;

using Binance.Net.Enums;
using Google.Protobuf.WellKnownTypes;
using Lampyris.CSharp.Common;
using System.Collections.Concurrent;

[Component]
public class IndicatorDataCacheService:ILifecycle
{
    /// <summary>
    /// 表示缓存的指标的数量，举个例子: 如果等于50，
    /// 则对于BTCUSDT的1m,15m,1D的k线，都缓存最近50根k线对应的指标数值
    /// 当新的一根k线产生的时候，最久的那根k线的指标数据会被移除，
    /// 为了方便实现这个功能，这里使用了循环队列式的数组
    /// </summary>
    private readonly int m_MaxCacheLength = 50;

    // 缓存结构：symbol -> BarSize -> 指标类型 -> 数据队列(该周期的价格是否已经结束(bool) + 价格队列 + 指标数值队列)
    // 存储一个价格队列的可以更方便的计算指标数值
    // 该周期的价格是否已经结束的bool值，其具体作用:
    // 1) 如果是true，则在下次收到最新价格时候，需要往队列末尾追加新的指标值，
    // 2) 如果是false, 则更新队列最末尾元素的值
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<BarSize, ConcurrentDictionary<string, Tuple<bool, CircularQueue<decimal>,CircularQueue<decimal>>>>> m_DataMap = new();

    /// <summary>
    /// 指标计算列表
    /// </summary>
    private List<IIndicatorCalculator> m_IndicatorCalculators = new List<IIndicatorCalculator>()
    {

    };

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="barSize">k线周期</param>
    /// <param name="indicatorType">指标类型字符串,由IIndicatorCalculator.ToString()得到</param>
    /// <param name="n">读取的数量，如果为非正值，则返回所有数据</param>
    /// <returns></returns>
    public ReadOnlySpan<decimal> GetIndicatorData(string symbol, BarSize barSize, string indicatorType,int n = -1)
    {
        if (m_DataMap.TryGetValue(symbol, out var barSizeMap) &&
            barSizeMap.TryGetValue(barSize, out var indicatorMap) &&
            indicatorMap.TryGetValue(indicatorType, out var data))
        {
            return data.Item3.AsLastSpan(n);
        }

        return null;
    }

    /// <summary>
    /// 更新价格数据，重新计算指标
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    /// <param name="value"></param>
    /// <param name="isEnd"></param>
    public void UpdatePrice(string symbol, BarSize barSize, double price, bool isEnd)
    {
        
    }

    /// <summary>
    /// 更新缓存数据
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    /// <param name="indicatorType"></param>
    /// <param name="value"></param>
    public void UpdateIndicatorData(string symbol, BarSize barSize, string indicatorType, decimal value)
    {
        var barSizeMap = m_DataMap.GetOrAdd(symbol, _ => new ());
        var indicatorMap = barSizeMap.GetOrAdd(barSize, _ => new ());
        var dataQueue = indicatorMap.GetOrAdd(indicatorType, _ => 
        new Tuple<bool, CircularQueue<decimal>, CircularQueue<decimal>>(false, 
                                                                        new CircularQueue<decimal>(m_MaxCacheLength), 
                                                                        new CircularQueue<decimal>(m_MaxCacheLength))).Item3;

        dataQueue.Enqueue(value); // 添加新数据
    }
}