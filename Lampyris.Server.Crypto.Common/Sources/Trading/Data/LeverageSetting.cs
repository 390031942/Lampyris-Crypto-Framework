using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lampyris.Server.Crypto.Common;

[DBTable("leverage_setting")]
public class LeverageSetting
{
    [DBColumn("symbol", "INTEGER", isPrimaryKey: true)] // 交易对
    public string Symbol { get; set; }

    [DBColumn("lerverage", "INTEGER")] // 杠杆倍数
    public int Leverage { get; set; }
}
