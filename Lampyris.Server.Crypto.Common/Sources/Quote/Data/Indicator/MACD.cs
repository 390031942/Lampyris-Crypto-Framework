namespace Lampyris.Server.Crypto.Common;

public class MACD : IIndicator
{
    private readonly int _shortPeriod;
    private readonly int _longPeriod;
    private readonly int _signalPeriod;

    public MACD(int shortPeriod = 12, int longPeriod = 26, int signalPeriod = 9)
    {
        _shortPeriod = shortPeriod;
        _longPeriod = longPeriod;
        _signalPeriod = signalPeriod;
    }

    public string Name => "MACD";

    public List<double> Calculate(List<QuoteCandleData> data)
    {
        var closePrices = data.Select(d => d.Close).ToList();

        // 计算短期EMA
        var shortEma = CalculateEMA(closePrices, _shortPeriod);

        // 计算长期EMA
        var longEma = CalculateEMA(closePrices, _longPeriod);

        // 计算DIF
        var dif = shortEma.Zip(longEma, (shortVal, longVal) => shortVal - longVal).ToList();

        // 计算DEA（Signal Line）
        var dea = CalculateEMA(dif, _signalPeriod);

        // 计算MACD柱（DIF - DEA）
        var macd = dif.Zip(dea, (difVal, deaVal) => difVal - deaVal).ToList();

        return macd;
    }

    public List<double> Query(List<QuoteCandleData> data, DateTime startTime, DateTime endTime)
    {
        var filteredData = data.Where(d => d.DateTime >= startTime && d.DateTime <= endTime).ToList();
        return Calculate(filteredData);
    }

    private List<double> CalculateEMA(List<double> prices, int period)
    {
        var ema = new List<double>();
        double multiplier = 2.0 / (period + 1);

        for (int i = 0; i < prices.Count; i++)
        {
            if (i == 0)
            {
                ema.Add(prices[i]); // 第一个值直接使用价格
            }
            else
            {
                ema.Add((prices[i] - ema[i - 1]) * multiplier + ema[i - 1]);
            }
        }

        return ema;
    }
}