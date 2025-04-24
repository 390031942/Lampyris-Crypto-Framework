namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;
using System;

[DBTable("order_status")]
public class OrderStatusInfo
{
    [DBColumn("order_id", "BIGINT", isPrimaryKey: true)] // 订单ID
    public long OrderId { get; set; }

    [DBColumn("order_bean", "JSON")] // 订单基本信息（存储为 JSON）
    public OrderInfo OrderInfo { get; set; }

    [DBColumn("status", "INTEGER")] // 订单状态
    public OrderStatus Status { get; set; }

    [DBColumn("filled_quantity", "DOUBLE")] // 已成交数量
    public decimal FilledQuantity { get; set; }

    [DBColumn("avg_filled_price", "DOUBLE")] // 成交均价
    public decimal AvgFilledPrice { get; set; }

    [DBColumn("api_order_ids", "JSON")] // API订单ID列表，仅仅在系统订单中有数据
    public List<long> ApiOrderIds { get; set; } = new List<long>();

    public List<OrderStatusInfo> ApiOrderStatusInfoList { get; set; } = new List<OrderStatusInfo>(); // API订单状态数据，仅仅在系统订单中有数据(不存数据库)

    public void Reset()
    {
        Status = OrderStatus.New;
        FilledQuantity = 0.0m;
        AvgFilledPrice = 0.0m;

        // API订单ID列表不重置，OrderStatusInfo列表重置
        // SubOrderIds.Clear();
        ApiOrderStatusInfoList.Clear();
    }

    public OrderStatusBean ToBean()
    {
        return new OrderStatusBean()
        {
            OrderId = OrderId,
            OrderBean = OrderInfo.ToBean(),
            Status = Status,
            FilledQuantity = (double)FilledQuantity,
            AvgFilledPrice = (double)AvgFilledPrice,
        };
    }
}