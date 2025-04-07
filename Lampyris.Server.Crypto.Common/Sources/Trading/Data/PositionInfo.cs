namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// USDT永续合约的持仓信息
/// </summary>
public class PositionInfo
{
    /// <summary>
    /// 交易对，例如 BTCUSDT
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// 持仓方向，long 或 short
    /// </summary>
    public string PositionSide { get; set; }

    /// <summary>
    /// 持仓数量
    /// </summary>
    public double PositionAmount { get; set; }

    /// <summary>
    /// 持仓未实现盈亏
    /// </summary>
    public double UnrealizedPnL { get; set; }

    /// <summary>
    /// 持仓杠杆倍数
    /// </summary>
    public int Leverage { get; set; }

    /// <summary>
    /// 持仓的初始保证金
    /// </summary>
    public double InitialMargin { get; set; }

    /// <summary>
    /// 持仓的维持保证金
    /// </summary>
    public double MaintenanceMargin { get; set; }

    /// <summary>
    /// 持仓的开仓价格
    /// </summary>
    public double CostPrice { get; set; }

    /// <summary>
    /// 当前标记价格
    /// </summary>
    public double MarkPrice { get; set; }

    /// <summary>
    /// 持仓是否被自动减仓
    /// </summary>
    public bool IsAutoDeleveraging { get; set; }

    /// <summary>
    /// 持仓的更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 重写ToString方法，便于调试和日志记录
    /// </summary>
    /// <returns>持仓信息的字符串表示</returns>
    public override string ToString()
    {
        return $"Symbol: {Symbol}, PositionSide: {PositionSide}, PositionAmount: {PositionAmount}, " +
                $"UnrealizedPnL: {UnrealizedPnL}, Leverage: {Leverage}, EntryPrice: {EntryPrice}, " +
                $"MarkPrice: {MarkPrice}, UpdateTime: {UpdateTime}";
    }
}
