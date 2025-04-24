namespace Lampyris.Server.Crypto.Common;

public class QuoteTickerData
{
    // 合约ID
    public string Symbol = "";

    // 最新成交价
    public decimal Price;

    // 最新成交的数量，0 代表没有成交量
    public decimal LastSize;

    // 24小时最高价
    public decimal High;

    // 24小时最低价
    public decimal Low;

    // 24小时成交量，以币为单位
    public decimal Volumn;

    // 24小时成交量，以张为单位
    public decimal Currency;

    // ticker数据产生时间，Unix时间戳的毫秒数格式，如 1597026383085
    public long Timestamp;

    // 涨幅
    public decimal ChangePerc;

    // 涨跌额
    public decimal Change;

    // 均价
    public decimal Avg => Volumn / Currency;

    // 标记价格
    public decimal MarkPrice;

    // 指数价格
    public decimal IndexPrice;

    // 资金费率
    public decimal FundingRate;

    // 下一次资金时间戳
    public long NextFundingTime;

    // 涨速
    public decimal RiseSpeed;

    // 异动标签
    public List<string> Labels;
}
