namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

public class OrderInfo
{
    public string            Symbol = "";
    public OrderSide         Side;
    public OrderPositionSide PositionSide;
    public OrderType         OrderType;
    public double            Price;
    public ExtraOrderInfo?   ExtraOrderInfo = null;
}

public class OrderStateInfo
{
    public string     Symbol = "";
    public string     OrderId = "";
    public string     ClientOrderId = "";
    public OrderSide  Side;
    public OrderType  OrderType;
    public OrderState Quantity;
    public double     Price;
    public double     FilledQuantity;
    public double     AveragePrice;
    public long       UpdateTime;
}

[Component]
public abstract class AbstractTradeService
{
    public virtual string PlaceOrder(OrderInfo orderInfo)
    {
        return PlaceOrderAsync(orderInfo).Result;
    }

    public virtual string CancelOrder(string symbol, string orderId)
    {
        return CancelOrderAsync(symbol,orderId).Result;
    }

    public abstract Task<string> PlaceOrderAsync(OrderInfo orderInfo);

    public abstract Task<string> CancelOrderAsync(string symbol, string orderId);
}
