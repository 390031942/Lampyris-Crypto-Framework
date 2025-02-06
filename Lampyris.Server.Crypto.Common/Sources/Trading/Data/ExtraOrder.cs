namespace Lampyris.Server.Crypto.Common;

public class ExtraOrderSubInfo
{
    public double TriggerPrice = -1;
    public double LimitPrice = -1;
}

public class ExtraOrderInfo
{
    public ExtraOrderSubInfo StopLossInfo   = new ExtraOrderSubInfo();
    public ExtraOrderSubInfo MakeProfitInfo = new ExtraOrderSubInfo();
}
