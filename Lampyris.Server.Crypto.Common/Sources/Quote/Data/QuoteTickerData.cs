namespace Lampyris.Server.Crypto.Common;

public class QuoteTickerData
{
    // 合约ID
    public string Symbol = "";

    // 最新成交价
    public double Price;

    // 最新成交的数量，0 代表没有成交量
    public double LastSize;

    // 24小时最高价
    public double High;

    // 24小时最低价
    public double Low;

    // 24小时成交量，以币为单位
    public double Volumn;

    // 24小时成交量，以张为单位
    public double Currency;

    // ticker数据产生时间，Unix时间戳的毫秒数格式，如 1597026383085
    public long Timestamp;

    // 涨幅
    public double ChangePerc;

    // 涨跌额
    public double Change;

    // 均价
    public double Avg => Volumn / Currency;

    // 标记价格
    public double MarkPrice;

    // 指数价格
    public double IndexPrice;

    // 资金费率
    public double FundingRate;

    // 下一次资金时间戳
    public long NextFundingTime;

    // 涨速
    public double RiseSpeed;

    // 异动标签
    public List<string> Labels;
}
