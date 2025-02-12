namespace Lampyris.Server.Crypto.Common;

// {0} symbol, {1} barSize
[DBTable("quote_candle_data_{0}_{1}")]
public class QuoteCandleData : IComparable<QuoteCandleData>
{
    [DBColumn("datetime", "DATETIME", isPrimaryKey: true)] // 开始时间，主键
    public DateTime DateTime { get; set; }

    [DBColumn("open", "DOUBLE")] // 开盘价格
    public double Open { get; set; }

    [DBColumn("close", "DOUBLE")] // 收盘价格
    public double Close { get; set; }

    [DBColumn("low", "DOUBLE")] // 最低价格
    public double Low { get; set; }

    [DBColumn("high", "DOUBLE")] // 最高价格
    public double High { get; set; }

    [DBColumn("volume", "DOUBLE")] // 交易量（张）
    public double Volume { get; set; }

    [DBColumn("currency", "DOUBLE")] // 交易量（USDT）
    public double Currency { get; set; }

    public int CompareTo(QuoteCandleData? other)
    {
        if (other == null)
        {
            return 0;
        }
        return other.DateTime.CompareTo(this.DateTime);
    }

    public double ChangePercentage(QuoteCandleData? other)
    {
        if (other == null)
        {
            return 0;
        }
        return Math.Round((other.Close - Close) / Close * 100, 2);
    }
}
