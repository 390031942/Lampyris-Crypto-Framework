namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;

[DBTable("order_status")]
public class OrderStatusInfo
{
    [DBColumn("order_id", "INTEGER", isPrimaryKey: true)] // 订单ID
    public int OrderId { get; set; }

    [DBColumn("order_bean", "TEXT")] // 订单基本信息（存储为 JSON）
    public OrderInfo OrderInfo { get; set; }

    [DBColumn("status", "INTEGER")] // 订单状态
    public OrderStatus Status { get; set; }

    [DBColumn("filled_quantity", "DOUBLE")] // 已成交数量
    public double FilledQuantity { get; set; }

    [DBColumn("avg_filled_price", "DOUBLE")] // 成交均价
    public double AvgFilledPrice { get; set; }
}