namespace Lampyris.Server.Crypto.Common;

public class CandleContinuousColorMonitor: IMarketAnomalyMonitor
{
    public override int CoolDownTime => 30 * 60 * 100; 

    public CandleContinuousColorMonitor()
    {
        AddCondition(new CandleContinuousColorCondition(BarSize._1m, true, 5));
        AddCondition(new CandleCurrencyCondition(BarSize._1m, 100000));
    }
}
