namespace Lampyris.Framework.Server.Common;

/*
 * 时间粒度，默认值1m
 * 如 [1m/3m/5m/15m/30m/1H/2H/4H]
 * 香港时间开盘价k线：[6H/12H/1D/2D/3D/1W/1M/3M]
 * UTC时间开盘价k线：[/6Hutc/12Hutc/1Dutc/2Dutc/3Dutc/1Wutc/1Mutc/3Mutc]
 */
public enum OkxBarSize
{
    [NamedValue("1m")]
    _1m,
    [NamedValue("3m")]
    _3m,
    [NamedValue("5m")]
    _5m,
    [NamedValue("15m")]
    _15m,
    [NamedValue("30m")]
    _30m,
    [NamedValue("1H")]
    _1H,
    [NamedValue("2H")]
    _2H,
    [NamedValue("4H")]
    _4H,
    [NamedValue("6h")]
    _6H,
    [NamedValue("12h")]
    _12H,
    [NamedValue("1D")]
    _1D,
    [NamedValue("2D")]
    _2D,
    [NamedValue("3D")]
    _3D,
    [NamedValue("1W")]
    _1W,
    [NamedValue("1M")]
    _1M,
    [NamedValue("3M")]
    _3M,
}