namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;
using Lampyris.CSharp.Common;

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
    public PositionSide PositionSide { get; set; }

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

    /// <summary>
    /// 持仓的初始保证金
    /// </summary>
    public decimal InitialMargin { get; set; }

    /// <summary>
    /// 持仓的维持保证金
    /// </summary>
    public decimal MaintenanceMargin { get; set; }

    /// <summary>
    /// 持仓的开仓价格
    /// </summary>
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 当前标记价格
    /// </summary>
    public decimal MarkPrice { get; set; }

    /// <summary>
    /// 持仓被自动减仓队列
    /// </summary>
    public int AutoDeleveragingLevel { get; set; }

    /// <summary>
    /// 持仓的更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// 强平价格
    /// </summary>
    public decimal LiquidationPrice { get; set; }

    /// <summary>
    /// 对于某个Symbol的系统持仓信息，其包含的若干个子账号的分仓持仓
    /// </summary>
    public List<PositionInfo> ApiPositionInfoList = new List<PositionInfo>();

    /// <summary>
    /// 重写ToString方法，便于调试和日志记录
    /// </summary>
    /// <returns>持仓信息的字符串表示</returns>
    public override string ToString()
    {
        return $"Symbol: {Symbol}, PositionSide: {PositionSide}, PositionAmount: {Quantity}, " +
                $"UnrealizedPnL: {UnrealizedPnL}, CostPrice: {CostPrice}, " +
                $"MarkPrice: {MarkPrice}, UpdateTime: {UpdateTime}";
    }

    public PositionBean ToBean()
    {
        return new PositionBean
        {
            Symbol = Symbol,
            PositionSide = PositionSide,
            Quantity = (double)Quantity,
            UnrealizedPnL = (double)UnrealizedPnL,
            RealizedPnL = (double)RealizedPnL,
            InitialMargin = (double)InitialMargin,
            MaintenanceMargin = (double)MaintenanceMargin,
            CostPrice = (double)CostPrice,
            MarkPrice = (double)MarkPrice,
            AutoDeleveragingLevel = AutoDeleveragingLevel,
            UpdateTime = DateTimeUtil.ToUnixTimestampMilliseconds(UpdateTime),
            LiquidationPrice = (double)LiquidationPrice,
        };
    }
}
