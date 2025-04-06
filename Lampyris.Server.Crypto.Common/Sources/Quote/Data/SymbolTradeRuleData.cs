using System;

namespace Lampyris.Server.Crypto.Common;

// {0} symbol
[DBTable("symbol_trade_rule_{0}")]
public class SymbolTradeRuleData : IComparable<SymbolTradeRuleData>
{
    [DBColumn("symbol", "VARCHAR(255)", isPrimaryKey: true)] // 交易对，主键
    public string Symbol { get; set; }

    [DBColumn("max_price", "DOUBLE")] // 最大价格
    public double MaxPrice { get; set; }

    [DBColumn("min_price", "DOUBLE")] // 最小价格
    public double MinPrice { get; set; }

    [DBColumn("price_tick_size", "DOUBLE")] // 价格最小变动单位
    public double PriceTickSize { get; set; }

    [DBColumn("max_quantity", "DOUBLE")] // 最大交易量
    public double MaxQuantity { get; set; }

    [DBColumn("min_quantity", "DOUBLE")] // 最小交易量
    public double MinQuantity { get; set; }

    [DBColumn("quantity_tick_size", "DOUBLE")] // 交易量最小变动单位
    public double QuantityTickSize { get; set; }

    [DBColumn("min_notional", "DOUBLE")] // 最小名义价值
    public double MinNotional { get; set; }

    /// <summary>
    /// 比较规则对象，按交易对名称排序。
    /// </summary>
    /// <param name="other">另一个 SymbolTradeRuleBean 对象</param>
    /// <returns>比较结果</returns>
    public int CompareTo(SymbolTradeRuleData? other)
    {
        if (other == null)
        {
            return 1;
        }
        return string.Compare(Symbol, other.Symbol, StringComparison.Ordinal);
    }

    /// <summary>
    /// 检查是否符合交易规则。
    /// </summary>
    /// <param name="price">价格</param>
    /// <param name="quantity">交易量</param>
    /// <returns>是否符合规则</returns>
    public bool IsValidTrade(double price, double quantity)
    {
        if (price < MinPrice || price > MaxPrice)
        {
            return false; // 价格超出范围
        }

        if (quantity < MinQuantity || quantity > MaxQuantity)
        {
            return false; // 交易量超出范围
        }

        if (price % PriceTickSize != 0)
        {
            return false; // 价格不符合最小变动单位
        }

        if (quantity % QuantityTickSize != 0)
        {
            return false; // 交易量不符合最小变动单位
        }

        if (price * quantity < MinNotional)
        {
            return false; // 成交额低于最小值
        }

        return true; // 符合规则
    }
}
