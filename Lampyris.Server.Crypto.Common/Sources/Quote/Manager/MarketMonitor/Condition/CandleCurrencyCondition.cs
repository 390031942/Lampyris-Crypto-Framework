namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// k线流最近一根 
/// </summary>
public class CandleCurrencyCondition : ICandleCondition
{
    public decimal Threshold { get; private set; }
    public override int ExpectedCount => 1;

    public CandleCurrencyCondition(BarSize barSize, decimal threshold) : base(barSize)
    {
        Threshold = threshold;
    }

    public bool Test(Span<QuoteCandleData> dataList)
    {
        return dataList[dataList.Length - 1].Currency >= (double)Threshold;
    }
}