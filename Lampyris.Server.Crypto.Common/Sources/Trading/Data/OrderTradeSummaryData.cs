namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 交易汇总结果,仅记录的了有交易的订单的结果，其在仅在订单结束后被统计
/// </summary>
public class OrderTradeSummaryData
{
    /// <summary>
    /// 交易对，例如 BTCUSDT
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// 订单 ID，用于标识具体的订单
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>
    /// 总已实现盈亏，表示该订单的所有交易记录的已实现盈亏汇总
    /// </summary>
    public decimal TotalRealizedPnL { get; set; }

    /// <summary>
    /// 总手续费，表示该订单的所有交易记录的手续费汇总
    /// </summary>
    public decimal TotalFee { get; set; }

    /// <summary>
    /// 总交易数量，表示该订单的所有交易记录的成交数量汇总
    /// </summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// 交易记录总数，表示该订单的所有交易记录的数量
    /// </summary>
    public int TradeCount { get; set; }
}
