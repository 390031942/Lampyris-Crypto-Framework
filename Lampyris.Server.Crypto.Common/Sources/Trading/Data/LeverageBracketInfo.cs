namespace Lampyris.Server.Crypto.Common;

[DBTable("lerverage_bracket")]
public class LeverageBracketInfo
{
    [DBColumn("symbol", "INTEGER", isPrimaryKey: true)] // 交易对，主键1
    public string Symbol { get; set; }

    [DBColumn("leverage", "INTEGER", isPrimaryKey: true)] // 当前分层下的最大杠杆倍数
    public int Leverage { get; set; }

    [DBColumn("notional_cap", "DOUBLE")] // 当前分层下的名义价值上限
    public double NotionalCap { get; set; }

    [DBColumn("notional_floor", "DOUBLE")] // 当前分层下的名义价值下限
    public double NotionalFloor { get; set; }
}
