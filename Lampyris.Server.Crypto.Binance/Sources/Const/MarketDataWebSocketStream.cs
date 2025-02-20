namespace Lampyris.Server.Crypto.Binance;

public static class MarketDataWebSocketStream
{
    /// <summary>
    /// 全市场的完整Ticker
    /// 所有symbol 24小时完整ticker信息.需要注意的是，只有发生变化的ticker更新才会被推送。
    /// </summary>
    public const string MARKET_TICKER = "!ticker@arr";

    /// <summary>
    /// 单个symbol归集交易
    /// 同一价格、同一方向、同一时间(100ms计算)的trade会被聚合为一条.
    /// 参数{0}: symbol
    /// </summary>
    public const string TRADE = "{0}@aggTrade";

    /// <summary>
    /// 有限档深度信息
    /// 推送有限档深度信息。levels表示几档买卖单信息, 可选 5/10/20档，默认为20档，更新速度100ms
    /// 参数{0}: symbol
    /// </summary>
    public const string DEPTH = "{0}@depth20@100ms";

    /// <summary>
    /// K线stream逐秒推送所请求的K线种类(最新一根K线)的更新。推送间隔250毫秒(如有刷新)
    /// 参数{0}: symbol
    /// 参数{1}: interval
    /// </summary>
    public const string KLINE = "{0}@kline_{1}";

    /// <summary>
    /// 最新标记价格
    /// 参数{0}: symbol
    /// </summary>
    public const string MARK_PRICE = "{0}@markPrice@1s";
}
