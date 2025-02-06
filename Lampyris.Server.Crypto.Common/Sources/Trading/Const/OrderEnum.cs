namespace Lampyris.Server.Crypto.Common;

public enum OrderSide
{
    Buy,
    Sell
}
public enum OrderType
{
    Limit, // 限价单
    Market,// 市价单
}

public enum OrderPositionSide
{
    Long, // 多
    Short,// 空
}

public enum OrderState
{
    New,
    Canceled,
    Calculated, // 订单 ADL 或爆仓
    Filled,     // 全部成交
    Expired,    // 过期
}
