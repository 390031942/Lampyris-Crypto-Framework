using Lampyris.Crypto.Protocol.Trading;
using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 条件单触发条件
/// </summary>
public interface IConditionTrigger
{
    /// <summary>
    /// 是否满足条件
    /// </summary>
    public bool IsSatisfied();
}

[DBTable("order")]
public class Order
{
    [DBColumn("symbol", "STRING")]
    public string Symbol { get; set; } // 交易对，例如 BTCUSDT

    [DBColumn("side", "INTEGER")]
    public TradeSide Side { get; set; } // 订单方向

    [DBColumn("order_type", "INTEGER")]
    public OrderType OrderType { get; set; } // 订单类型

    [DBColumn("quantity", "DOUBLE")]
    public double Quantity { get; set; } // 订单数量（以标的为单位）

    [DBColumn("cash_quantity", "DOUBLE")]
    public double CashQuantity { get; set; } // 订单数量（以USDT为单位）

    [DBColumn("price", "DOUBLE")]
    public double Price { get; set; } // 订单价格（限价单需要）

    [DBColumn("tif_type", "INTEGER")]
    public TimeInForceType TifType { get; set; } // 订单有效方式

    [DBColumn("good_till_date", "INTEGER")]
    public long GoodTillDate { get; set; } // TIF为GTD时订单的自动取消时间

    [DBColumn("reduce_only", "BOOLEAN")]
    public bool ReduceOnly { get; set; } // 是否只减仓

    [DBColumn("condition", "STRING")]
    public List<ConditionTriggerBean> Condition { get; set; } // 条件列表

    [DBColumn("createdTime", "DATETIME")]
    public DateTime CreatedTime { get; set; } // 创建时间

    /// <summary>
    /// 将 OrderBean 转换为 Order 对象
    /// </summary>
    /// <param name="bean">OrderBean 对象</param>
    /// <returns>Order 对象</returns>
    public static Order ValueOf(OrderBean bean)
    {
        if (bean == null)
        {
            throw new ArgumentNullException(nameof(bean), "OrderBean cannot be null");
        }

        return new Order
        {
            Symbol = bean.Symbol,
            Side = bean.Side,
            OrderType = bean.OrderType,
            Quantity = bean.Quantity,
            CashQuantity = bean.CashQuantity,
            Price = bean.Price,
            TifType = bean.TifType,
            GoodTillDate = bean.GoodTillDate,
            ReduceOnly = bean.ReduceOnly,
            Condition = new List<ConditionTriggerBean>(bean.Condition),
            CreatedTime = DateTimeUtil.FromUnixTimestamp(bean.CreatedTime),
        };
    }
}
