namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

public class MovingAverage : IIndicatorCalculator
{
    private readonly int m_Period;

    public MovingAverage(int period)
    {
        m_Period = period;
    }

    /// <summary>
    /// 计算均线的值
    /// </summary>
    /// <param name="prices">价格循环列表</param>
    /// <param name="indicateValues">MA均线指标的值</param>
    /// <param name="append">是否追加</param>
    public void CalculateAndStore(CircularQueue<decimal> prices, CircularQueue<decimal[]> indicateValues, bool append)
    {
        if (prices.Count <= 0)
        {
            return;
        }

        decimal sum = 0.0m;

        // 如果没数据就全部重新计算
        if (indicateValues.Count == 0)
        {
            for (int i = 0; i < prices.Count; i++)
            {
                sum += prices[i];
                if (i < m_Period)
                {
                    indicateValues.Enqueue(new decimal[] { decimal.MaxValue });
                }
                else
                {
                    indicateValues.Enqueue(new decimal[] { sum / m_Period });
                    sum -= prices[i - m_Period];
                }
            }
        }
        else
        {
            // 只计算最新的值
            for (int i = 0; i < m_Period; i++)
            {
                sum += prices[prices.Count - m_Period + i];
            }
            decimal avg = sum / m_Period;
            if (append)
            {
                indicateValues.Enqueue(new decimal[] { avg });
            }
            else
            {
                if (indicateValues.Count > 0)
                {
                    indicateValues[indicateValues.Count - 1] = new decimal[] { avg };
                }
            }
        }
    }

    public override string ToString()
    {
        return "MA" + m_Period;
    }
}
