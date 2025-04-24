namespace Lampyris.Server.Crypto.Common;

public class QuoteTradeData
{
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }

    public DateTime TradeTime { get; set; }
    public bool BuyerIsMaker { get; set; }
}
