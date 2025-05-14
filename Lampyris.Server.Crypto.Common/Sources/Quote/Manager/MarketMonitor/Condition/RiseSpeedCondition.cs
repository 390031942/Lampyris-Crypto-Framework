namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 涨速条件
/// </summary>
public class RiseSpeedCondition : ITickerCondition
{
    public decimal Threshold { get; private set; }
    public bool Greater { get; private set; }

    public RiseSpeedCondition(decimal threshold, bool greater)
    {
        Threshold = threshold;
        Greater = greater;
    }

    public override bool Test(QuoteTickerData quoteTickerData, out decimal value)
    {
        value = 0m;

        if (quoteTickerData == null)
        {
            return false;
        }


        return Greater ?
            quoteTickerData.RiseSpeed >= Threshold :
            quoteTickerData.RiseSpeed <= Threshold;
    }
}
