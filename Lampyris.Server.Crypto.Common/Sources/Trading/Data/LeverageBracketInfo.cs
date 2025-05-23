﻿using Lampyris.Crypto.Protocol.Trading;

namespace Lampyris.Server.Crypto.Common;

[DBTable("lerverage_bracket")]
public class LeverageBracketInfo
{
    [DBColumn("Symbol", "INTEGER", isPrimaryKey: true)] // 交易对，主键1
    public string Symbol { get; set; }

    [DBColumn("leverage", "INTEGER", isPrimaryKey: true)] // 当前分层下的最大杠杆倍数
    public int Leverage { get; set; }

    [DBColumn("notional_cap", "DOUBLE")] // 当前分层下的名义价值上限
    public decimal NotionalCap { get; set; }

    [DBColumn("notional_floor", "DOUBLE")] // 当前分层下的名义价值下限
    public decimal NotionalFloor { get; set; }

    public LeverageBracketBean ToBean()
    {
        LeverageBracketBean bean = new LeverageBracketBean();
        bean.Leverage = Leverage;
        bean.NotionalCap = Convert.ToDouble(NotionalCap);
        bean.NotionalFloor = Convert.ToDouble(NotionalFloor);
        return bean;
    }
}
