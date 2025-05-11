using Lampyris.Crypto.Protocol.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lampyris.Server.Crypto.Common;

[DBTable("leverage_setting")]
public class LeverageSetting
{
    [DBColumn("Symbol", "INTEGER", isPrimaryKey: true)] // 交易对
    public string Symbol { get; set; }

    [DBColumn("lerverage", "INTEGER")] // 杠杆倍数
    public int Leverage { get; set; }


    [DBColumn("maxNotional", "DECIMAL")] // 当前杠杆倍数下最大开仓名义价值
    public decimal MaxNotional { get; set; }

    public LeverageBean ToBean()
    {
        LeverageBean bean = new LeverageBean();
        bean.Symbol = Symbol;
        bean.Leverage = Leverage;
        bean.MaxNotional = Convert.ToDouble(MaxNotional);
        return bean;
    }
}
