namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 区间放量条件: IntervalLength1内的平均成交量 * 倍数Times < IntervalLength2分内的平均成交量 
/// </summary>
public class IntervalVolumeSurgeCondition : ICandleCondition
{
    public IntervalVolumeSurgeCondition(BarSize barSize, int interval1, int interval2, int times) : base(barSize)
    {
        IntervalLength1 = interval1;
        IntervalLength2 = interval2;
        Times = times;
    }

    public int IntervalLength1 { get; private set; }
    public int IntervalLength2 { get; private set; }

    public int Times { get; private set; }

    public override int ExpectedCount => IntervalLength1;

    public override bool Test(ReadOnlySpan<QuoteCandleData> dataList, bool isEnd, out decimal value)
    {
        value = 0m;

        // IntervalLength1分钟内的平均成交额
        double avg1 = 0;

        // IntervalLength2分钟内的平均成交额
        double avg2 = 0;

        int index = 0;
        foreach (QuoteCandleData data in dataList)
        {
            avg1 += data.Currency;

            if (index >= ExpectedCount - IntervalLength2)
            {
                avg2 += data.Currency;
            }
            index++;
        }

        return avg1 * IntervalLength2 * Times < avg2 * IntervalLength1;
    }
}
