namespace Lampyris.Server.Crypto.Common;

public class MovingAverage : IIndicator
{
    private readonly int _period;

    public MovingAverage(int period)
    {
        _period = period;
    }

    public string Name => $"MA{_period}";

    // 计算MA指标
    public List<double> Calculate(List<QuoteCandleData> data)
    {
        var result = new List<double>();
        for (int i = 0; i < data.Count; i++)
        {
            if (i < _period - 1)
            {
                result.Add(double.NaN); // 前期数据不足，返回NaN
            }
            else
            {
                double sum = 0;
                for (int j = i; j > i - _period; j--)
                {
                    sum += data[j].Close;
                }
                result.Add(sum / _period);
            }
        }
        return result;
    }

    // 查询某一段K线的MA指标
    public List<double> Query(List<QuoteCandleData> data, DateTime startTime, DateTime endTime)
    {
        var filteredData = data.Where(d => d.DateTime >= startTime && d.DateTime <= endTime).ToList();
        return Calculate(filteredData);
    }
}
