namespace Lampyris.Server.Crypto.Common;

/*
 * 时间粒度，默认值1m
 * 如 [1m/3m/5m/15m/30m/1H/2H/4H]
 */
public enum BarSize
{
    _1m,  _3m, _5m, _15m,
    _30m, _1H, _2H, _4H,
    _6H, _12H, _1D, _2D,
    _3D, _1W,  _1M, _3M,
}