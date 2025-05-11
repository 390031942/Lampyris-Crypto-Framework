namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// k�������һ��K�ߵĳɽ���
/// </summary>
public class CandleCurrencyCondition : ICandleCondition
{
    public decimal Threshold { get; private set; }
    public override int ExpectedCount => 1;

    public CandleCurrencyCondition(BarSize barSize, decimal threshold) : base(barSize)
    {
        Threshold = threshold;
    }

    public override bool Test(ReadOnlySpan<QuoteCandleData> dataList, bool isEnd)
    {
        return dataList[dataList.Length - 1].Currency >= (double)Threshold;
    }
}