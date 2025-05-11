namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// k线连续红/绿条件
/// </summary>
public class CandleContinuousColorCondition : ICandleCondition
{
    public int Threshold { get; private set; }

    public bool IsRise { get; private set; }

    public override int ExpectedCount => Threshold;

    public CandleContinuousColorCondition(BarSize barSize, bool isRise, int threshold) : base(barSize)
    {
        Threshold = threshold;
        IsRise = isRise;
    }

    public override bool Test(ReadOnlySpan<QuoteCandleData> dataList, bool isEnd)
    {
        foreach(QuoteCandleData data in dataList)
        {
            if(!(data.ChangePerc > 0 && IsRise) && !(data.ChangePerc < 0 && !IsRise))
            {
                return false;
            }
        }

        return true;
    }
}
