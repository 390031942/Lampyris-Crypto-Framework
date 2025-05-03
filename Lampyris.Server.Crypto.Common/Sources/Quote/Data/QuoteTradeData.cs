namespace Lampyris.Server.Crypto.Common;

public class QuoteTradeData
{
    /// <summary>
    /// 价格
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    public decimal Currency => Price * Quantity;

    /// <summary>
    /// 交易笔数
    /// </summary>
    public int TradeCount { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    public DateTime TradeTime { get; set; }

    /// <summary>
    /// 表示买方是 Maker，即买方挂出了限价买单，卖方是 Taker。
    /// </summary>
    public bool BuyerIsMaker { get; set; }
}
