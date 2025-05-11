namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.Collections.Concurrent;

[Component]
public class IndicatorDataCacheService : ILifecycle
{
    /// <summary>
    /// 表示缓存的指标的数量，举个例子: 如果等于50，
    /// 则对于BTCUSDT的1m,15m,1D的k线，都缓存最近50根k线对应的指标数值
    /// 当新的一根k线产生的时候，最久的那根k线的指标数据会被移除，
    /// 为了方便实现这个功能，这里使用了循环队列式的数组
    /// </summary>
    public readonly int MaxCacheLength = 50;

    /// <summary>
    /// 每个交易对和周期的指标数据
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<BarSize, PerSymbolBarSizeIndicatorData>> m_DataMap = new();

    /// <summary>
    /// 指标计算列表
    /// </summary>
    private List<IIndicatorCalculator> m_IndicatorCalculators = new List<IIndicatorCalculator>()
    {
        new MovingAverage(5),
        new MovingAverage(10),
        new MovingAverage(20),
        new MACD(),
    };

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="barSize">k线周期</param>
    /// <param name="indicatorType">指标类型字符串,由IIndicatorCalculator.ToString()得到</param>
    /// <param name="n">读取的数量，如果为非正值，则返回所有数据</param>
    /// <returns></returns>
    public ReadOnlySpan<decimal[]> GetIndicatorData(string symbol, BarSize barSize, string indicatorType, int n = -1)
    {
        if (m_DataMap.TryGetValue(symbol, out var barSizeMap) &&
            barSizeMap.TryGetValue(barSize, out var data) &&
            data.IndicatorMap.TryGetValue(indicatorType, out var indicatorData))
        {
            return indicatorData.AsLastSpan(n);
        }

        return null;
    }

    /// <summary>
    /// 外部调用-更新价格数据，重新计算指标
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    /// <param name="price"></param>
    /// <param name="isEnd"></param>
    public void UpdateIndicator(string symbol, BarSize barSize, decimal price, bool isEnd)
    {
        var barSizeMap = m_DataMap.GetOrAdd(symbol, _ => new());
        var data = barSizeMap.GetOrAdd(barSize, _ => new PerSymbolBarSizeIndicatorData(MaxCacheLength));

        if (isEnd == data.IsEnd)
        {
            throw new InvalidDataException($"Failed to update indicator for symbol = \"{symbol}\", barSize = \"{barSize}\", two consecutive \"isEnd\" appeared...");
        }

        bool isEndBefore = data.IsEnd;
        data.IsEnd = isEnd;

        foreach (IIndicatorCalculator indicatorCalculator in m_IndicatorCalculators)
        {
            string? indicatorName = indicatorCalculator.ToString();
            if (!string.IsNullOrEmpty(indicatorName))
            {
                var indicatorDataList = data.IndicatorMap.GetOrAdd(indicatorName, _ => new CircularQueue<decimal[]>(MaxCacheLength));

                if(isEndBefore)
                {
                    data.PriceDataList[data.PriceDataList.Count - 1] = price;
                    indicatorCalculator.CalculateAndStore(data.PriceDataList, indicatorDataList, false);
                }
                else
                {
                    data.PriceDataList.Enqueue(price);
                    indicatorCalculator.CalculateAndStore(data.PriceDataList, indicatorDataList, true);
                }
            }
        }
    }

    /// <summary>
    /// 外部调用-更新价格数据列表，重新计算指标(在k线数据 完整性验证 完毕后调用)
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    /// <param name="priceList"></param>
    public void UpdatePriceList(string symbol, BarSize barSize, IEnumerable<decimal> priceList)
    {
        var barSizeMap = m_DataMap.GetOrAdd(symbol, _ => new());
        var data = barSizeMap.GetOrAdd(barSize, _ => new PerSymbolBarSizeIndicatorData(MaxCacheLength));

        foreach (decimal price in priceList)
        {
            data.PriceDataList.Enqueue(price);
        }

        foreach (IIndicatorCalculator indicatorCalculator in m_IndicatorCalculators)
        {
            string? indicatorName = indicatorCalculator.ToString();
            if (!string.IsNullOrEmpty(indicatorName))
            {
                var indicatorDataList = data.IndicatorMap.GetOrAdd(indicatorName, _ => new CircularQueue<decimal[]>(MaxCacheLength));
                indicatorCalculator.CalculateAndStore(data.PriceDataList, indicatorDataList, false);
            }
        }
    }

    /// <summary>
    /// 封装的内部类，用于存储每个交易对和周期的指标数据
    /// </summary>
    private class PerSymbolBarSizeIndicatorData
    {
        /// <summary>
        /// 当前周期的价格是否已经结束
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// 价格数据队列
        /// </summary>
        public CircularQueue<decimal> PriceDataList { get; }

        /// <summary>
        /// 指标数据映射
        /// </summary>
        public ConcurrentDictionary<string, CircularQueue<decimal[]>> IndicatorMap { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxCacheLength">最大缓存长度</param>
        public PerSymbolBarSizeIndicatorData(int maxCacheLength)
        {
            this.IsEnd = false;
            this.PriceDataList = new CircularQueue<decimal>(maxCacheLength);
            this.IndicatorMap = new ConcurrentDictionary<string, CircularQueue<decimal[]>>();
        }
    }
}
