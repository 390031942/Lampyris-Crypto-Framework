namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;

/// <summary>
/// USDT永续合约的历史持仓信息
/// 一次历史仓位被定义为：从首个开仓订单开始到最后一个完全平仓的订单之间的持仓记录
/// </summary>
[DBTable("historical_positions")]
public class HistoricalPositionInfo
{
    /// <summary>
    /// 交易对，例如 BTCUSDT
    /// </summary>
    [DBColumn("symbol", "STRING")]
    public string Symbol { get; set; }

    /// <summary>
    /// 持仓方向，long 或 short
    /// </summary>
    [DBColumn("position_side", "INTERGER")]
    public PositionSide PositionSide { get; set; }

    /// <summary>
    /// 持仓已实现盈亏
    /// </summary>
    [DBColumn("realized_pnl", "DECIMAL")]
    public decimal RealizedPnL { get; set; }

    /// <summary>
    /// 持仓的平均开仓价格
    /// </summary>
    [DBColumn("avg_open_price", "DECIMAL")]
    public decimal AvgOpenPrice { get; set; }

    /// <summary>
    /// 持仓的平均平仓价格
    /// </summary>
    [DBColumn("avg_close_price", "DECIMAL")]
    public decimal AvgClosePrice { get; set; }

    /// <summary>
    /// 持仓期间系统订单id列表
    /// </summary>
    [DBColumn("order_ids", "JSON")]
    public List<long> CorrespondingOrderIdList { get; set; } = new List<long>();

    /// <summary>
    /// 持仓的交易手续费
    /// </summary>
    [DBColumn("fee", "DECIMAL")]
    public decimal Fee { get; set; }
}