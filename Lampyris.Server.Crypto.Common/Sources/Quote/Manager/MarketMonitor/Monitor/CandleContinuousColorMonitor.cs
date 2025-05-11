namespace Lampyris.Server.Crypto.Common;

public class CandleContinuousColorMonitor: IMarketAnomalyMonitor
{
    public override int CoolDownTime => 30 * 60 * 100; 

    public CandleContinuousColorMonitor()
    {
        CandleContinuousColorCondition condition1 = new CandleContinuousColorCondition(BarSize._1m, true, 5);
        CandleCurrencyCondition condition2 = new CandleCurrencyCondition(BarSize._1m, 100000);
    }

    protected override bool CheckImpl(string symbol)
    {
    }
}
