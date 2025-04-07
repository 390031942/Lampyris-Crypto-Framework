namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;
using Newtonsoft.Json;

[DBTable("order")]
public class OrderInfo
{
    [DBColumn("userId", "INTEGER", isPrimaryKey: true)] // 创建者用户ID, 主键1
    public string ClientUserId = "";

    [DBColumn("id", "INTEGER", isPrimaryKey: true)] // 订单ID, 主键2
    public string OrderId = "";

    [DBColumn("symbol", "STRING")] // 交易对，例如 BTCUSDT
    public string Symbol = "";

    [DBColumn("side", "INTEGER")] // 订单方向
    public TradeSide Side;

    [DBColumn("order_type", "INTEGER")] // 订单类型
    public OrderType OrderType;

    [DBColumn("quantity", "DOUBLE")] // 订单数量（以标的为单位）
    public double Quantity;

    [DBColumn("cash_quantity", "DOUBLE")] // 订单数量（以USDT为单位）
    public double CashQuantity;

    [DBColumn("price", "DOUBLE")] // 订单价格（限价单需要）
    public double Price;

    [DBColumn("tif_type", "INTEGER")] // 订单有效方式
    public TimeInForceType TifType;

    [DBColumn("good_till_date", "BIGINT")] // TIF为GTD时订单的自动取消时间
    public long GoodTillDate;

    [DBColumn("reduce_only", "BOOLEAN")] // 是否只减仓
    public bool ReduceOnly;

    [DBColumn("condition", "JSON")] // 条件列表
    public List<ConditionTriggerBean> Condition = new List<ConditionTriggerBean>();

    [DBColumn("created_time", "BIGINT")] // 创建时间
    public long CreatedTime;

    // 重写 ToString 方法
    public override string ToString()
    {
        // 使用 Newtonsoft.Json 将对象序列化为 JSON 字符串
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
