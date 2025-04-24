using Lampyris.Crypto.Protocol.Quote;

namespace Lampyris.Server.Crypto.Common;

public class SymbolTradeRule
{
    /// <summary>
    /// 交易对，例如 BTCUSDT
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    /// 最小价格
    /// </summary>
    public decimal MinPrice { get; set; }

    /// <summary>
    /// 最大价格
    /// </summary>
    public decimal MaxPrice { get; set; }

    /// <summary>
    /// 价格步进
    /// </summary>
    public decimal PriceStep { get; set; }

    /// <summary>
    /// 最小数量
    /// </summary>
    public decimal MinQuantity { get; set; }

    /// <summary>
    /// 最大数量
    /// </summary>
    public decimal MaxQuantity { get; set; }

    /// <summary>
    /// 数量步进
    /// </summary>
    public decimal QuantityStep { get; set; }

    /// <summary>
    /// 最小名义价值
    /// </summary>
    public decimal MinNotional { get; set; }

    /// <summary>
    /// 上架时间
    /// </summary>
    public long OnBoardTimestamp { get; set; }

    public SymbolTradeRuleBean ToBean()
    {
        return new SymbolTradeRuleBean()
        {
            Symbol = Symbol,
            MaxPrice = Convert.ToDouble(MaxPrice),
            MinPrice = Convert.ToDouble(MinPrice),
            PriceTickSize = Convert.ToDouble(PriceStep),
            MaxQuantity = Convert.ToDouble(MaxQuantity),
            MinQuantity = Convert.ToDouble(MinQuantity),
            QuantityTickSize = Convert.ToDouble(QuantityStep),
            MinNotional = Convert.ToDouble(MinNotional),
            OnBoardTimestamp = OnBoardTimestamp,
        };
    }
}