using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

public class MACD : IIndicatorCalculator
{
    private readonly int _shortPeriod;  // 短期 EMA 的周期
    private readonly int _longPeriod;   // 长期 EMA 的周期
    private readonly int _signalPeriod; // 信号线（DEA）的周期

    public MACD(int shortPeriod = 12, int longPeriod = 26, int signalPeriod = 9)
    {
        _shortPeriod = shortPeriod;
        _longPeriod = longPeriod;
        _signalPeriod = signalPeriod;
    }

    /// <summary>
    /// 计算 MACD 的值并存储到缓存中
    /// </summary>
    /// <param name="prices">价格循环列表</param>
    /// <param name="indicateValues">存储 MACD 值的循环列表</param>
    /// <param name="append">是否追加</param>
    public void CalculateAndStore(
        CircularQueue<decimal> prices,
        CircularQueue<decimal[]> indicateValues,
        bool append)
    {
        if (prices.Count <= 0)
        {
            return;
        }

        decimal shortEma = 0;
        decimal longEma = 0;
        decimal dif = 0;
        decimal dea = 0;
        decimal macd = 0;

        // 如果没有足够的数据计算 MACD，则填充无效值
        if (prices.Count < _longPeriod)
        {
            for (int i = 0; i < prices.Count; i++)
            {
                indicateValues.Enqueue(new decimal[] { decimal.MaxValue, decimal.MaxValue, decimal.MaxValue });
            }
            return;
        }

        // 计算短期 EMA 和长期 EMA
        shortEma = CalculateEMA(prices, _shortPeriod);
        longEma = CalculateEMA(prices, _longPeriod);

        // 计算 DIF
        dif = shortEma - longEma;

        // 计算 DEA
        decimal previousDea = indicateValues.Count > 0 ? indicateValues[indicateValues.Count - 1][1] : 0;
        dea = previousDea + (dif - previousDea) * (2.0m / (_signalPeriod + 1));

        // 计算 MACD
        macd = 2 * (dif - dea);

        // 存储结果
        if (append)
        {
            indicateValues.Enqueue(new decimal[] { dif, dea, macd });
        }
        else
        {
            if (indicateValues.Count > 0)
            {
                indicateValues[indicateValues.Count - 1] = new decimal[] { dif, dea, macd };
            }
        }
    }

    /// <summary>
    /// 计算 EMA（指数移动平均线）
    /// </summary>
    /// <param name="prices">价格循环列表</param>
    /// <param name="period">EMA 的周期</param>
    /// <returns>EMA 值</returns>
    private decimal CalculateEMA(CircularQueue<decimal> prices, int period)
    {
        decimal multiplier = 2.0m / (period + 1);
        decimal ema = prices[0]; // 初始化 EMA 为第一个价格

        for (int i = 1; i < prices.Count; i++)
        {
            ema = (prices[i] - ema) * multiplier + ema;
        }

        return ema;
    }

    public override string ToString()
    {
        return "MACD";
    }
}
