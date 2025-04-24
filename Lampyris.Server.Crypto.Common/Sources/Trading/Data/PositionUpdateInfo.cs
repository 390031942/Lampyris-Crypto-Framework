namespace Lampyris.Server.Crypto.Common;

/*
 * 用于更新持仓信息的，简要的PositionInfo
 */
public class PositionUpdateInfo
{
    /// <summary>
    /// 交易对，例如 BTCUSDT
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// 持仓数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 持仓未实现盈亏
    /// </summary>
    public decimal UnrealizedPnL { get; set; }

    /// <summary>
    /// 持仓已实现盈亏
    /// </summary>
    public decimal RealizedPnL { get; set; }

    /// 持仓的开仓价格
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 持仓的更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
}
