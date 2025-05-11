namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;
using Newtonsoft.Json;
using System;

[DBTable("order")]
public class OrderInfo
{
    [DBColumn("id", "BIGINT", isPrimaryKey: true)] // 订单ID, 主键(如果是存于数据库的订单，则需注意并非API订单ID，而是由Lampyris服务端产生的ID)
    public long OrderId;

    [DBColumn("userId", "INTEGER", isPrimaryKey: true)] // 创建者客户端ID
    public int ClientUserId = 1;

    [DBColumn("Symbol", "STRING")] // 交易对，例如 BTCUSDT
    public string Symbol = "";

    [DBColumn("side", "INTEGER")] // 订单方向
    public OrderSide Side;

    [DBColumn("position_side", "INTEGER")] // 持仓方向
    public PositionSide PositionSide;

    [DBColumn("order_type", "INTEGER")] // 订单类型
    public OrderType OrderType;

    [DBColumn("quantity", "decimal")] // 订单数量（以标的为单位）
    public decimal Quantity;

    [DBColumn("cash_quantity", "decimal")] // 订单数量（以USDT为单位）
    public decimal CashQuantity;

    [DBColumn("price", "decimal")] // 订单价格（限价单需要）
    public decimal Price;

    [DBColumn("tif_type", "INTEGER")] // 订单有效方式
    public TimeInForceType TifType;

    [DBColumn("good_till_date", "BIGINT")] // TIF为GTD时订单的自动取消时间
    public long GoodTillDate;

    [DBColumn("reduce_only", "BOOLEAN")] // 是否只减仓
    public bool ReduceOnly;

    [DBColumn("condition", "JSON")] // 条件列表
    public List<ConditionTriggerData> Condition = new List<ConditionTriggerData>();

    [DBColumn("created_time", "BIGINT")] // 创建时间
    public long CreatedTime;

    public static OrderInfo ValueOf(OrderBean bean)
    {
        if (bean == null)
        {
            throw new ArgumentNullException(nameof(bean), "OrderBean cannot be null");
        }

        return new OrderInfo
        {
            OrderId = -1,
            ClientUserId = -1,
            Symbol = bean.Symbol,
            Side = bean.Side,
            PositionSide = bean.PositionSide,
            OrderType = bean.OrderType,
            Quantity = (decimal)bean.Quantity,
            CashQuantity = (decimal)bean.CashQuantity,
            Price = (decimal)bean.Price,
            TifType = bean.TifType,
            GoodTillDate = bean.GoodTillDate,
            ReduceOnly = bean.ReduceOnly,
            Condition = bean.Condition.Select(c => new ConditionTriggerData
            {
                Type = c.Type,
                Value = c.Value
            }).ToList(),
            CreatedTime = bean.CreatedTime
        };
    }

    public OrderBean ToBean()
    {
        var bean = new OrderBean
        {
            Symbol = Symbol,
            Side = Side,
            PositionSide = PositionSide,
            OrderType = OrderType,
            Quantity = (double)Quantity,
            CashQuantity = (double)CashQuantity,
            Price = (double)Price,
            TifType = TifType,
            GoodTillDate = GoodTillDate,
            ReduceOnly = ReduceOnly,
            CreatedTime = CreatedTime
        };

        foreach(var condition in Condition)
        {
            bean.Condition.Add(new ConditionTriggerBean()
            {
                Type = condition.Type,
                Value = condition.Value
            });
        }

        return bean;
    }

    // 重写 ToString 方法
    public override string ToString()
    {
        // 使用 Newtonsoft.Json 将对象序列化为 JSON 字符串
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
