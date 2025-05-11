namespace Lampyris.Server.Crypto.Common;

[DBTable("self_selected_symbol")]
public class SelfSelectedSymbolData
{
    [DBColumn("id", "INTEGER", isPrimaryKey: true)] // 用户ID，主键1
    public string UserId { get; set; }

    [DBColumn("group_name", "STRING", isPrimaryKey: true)] // 组名，主键2
    public string GroupName { get; set; }

    [DBColumn("Symbol", "STRING", isPrimaryKey: true)] // 交易对，主键
    public string Symbol { get; set; }

    [DBColumn("Timestamp", "BIGINT")] // 自选时间戳
    public long Timestamp { get; set; }

    [DBColumn("initial_price", "DOUBLE")] // 自选价格
    public double InitialPrice { get; set; }
}
