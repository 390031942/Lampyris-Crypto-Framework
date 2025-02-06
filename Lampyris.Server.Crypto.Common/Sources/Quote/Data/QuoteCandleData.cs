namespace Lampyris.Server.Crypto.Common;

public class QuoteCandleData:IComparable<QuoteCandleData>
{
    // 开始时间
    public DateTime DateTime;

    // 开盘价格
    public double Open;

    // 最高价格
    public double Close;

    // 最低价格
    public double Low;

    // 收盘价格
    public double High;

    /*
     * 交易量，以张为单位
     * 如果是衍生品合约，数值为合约的张数。
     * 如果是币币/币币杠杆，数值为交易货币的数量。
     */
    public double Vol;

    /*
     * 交易量，以币为单位
     * 如果是衍生品合约，数值为交易货币的数量。
     * 如果是币币/币币杠杆，数值为计价货币的数量。
     */
    public double VolCcy;

    /*
     * 交易量，以计价货币为单位
     * 如 BTC-USDT和BTC-USDT-SWAP，单位均是USDT。
     * BTC-USD-SWAP单位是USD。
     */
    public double VolCcyQuote;

    // 均线值
    public double MA5;
    public double MA10;
    public double MA20;

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
        return Math.Round((other.Close - Close) / Close * 100,2);
    }
}
