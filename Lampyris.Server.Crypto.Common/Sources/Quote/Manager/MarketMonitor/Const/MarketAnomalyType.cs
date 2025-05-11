namespace Lampyris.Server.Crypto.Common;

public enum MarketAnomalyType
{
    /// <summary>
    /// k线连红连绿
    /// </summary>
    CANDLE_CONTINUOUS_COLOR = 0,

    /// <summary>
    /// k线区间放量
    /// </summary>
    CANDLE_INTERVAL_VOLUME_SURGE = 1,

    /// <summary>
    /// 逐笔成交异常增大
    /// </summary>
    TRADE_SURGE = 2,

    /// <summary>
    /// 涨速异常 
    /// </summary>
    RISE_SPEED = 3,
}
