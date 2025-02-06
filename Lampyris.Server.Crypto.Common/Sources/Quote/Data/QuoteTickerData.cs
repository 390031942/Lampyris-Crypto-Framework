namespace Lampyris.Server.Crypto.Common;

public class QuoteTickerData
{
    // 产品类型
    public InstType InstType;
    
    // 产品ID，如LTC-USD-SWAP
    public string InstId = "";
    
    // 最新成交价
    public double Last;
    
    // 最新成交的数量，0 代表没有成交量
    public double LastSz;
    
    // 卖一价
    public double AskPx;
    
    // 卖一价的挂单数数量
    public double AskSz;
    
    // 买一价
    public double BidPx;
    
    // 买一价的挂单数量
    public double BidSz;
    
    // 24小时开盘价
    public double Open24h;
    
    // 24小时最高价
    public double High24h;
    
    // 24小时最低价
    public double Low24h;
    
    // 24小时成交量，以币为单位
    public double VolCcy24h;
    
    // 24小时成交量，以张为单位
    public double Vol24h;
    
    // UTC 0 时开盘价
    public double SodUtc0;
    
    // UTC+8 时开盘价
    public double SodUtc8;
    
    // ticker数据产生时间，Unix时间戳的毫秒数格式，如 1597026383085
    public long Ts;

    // 涨幅
    public double Percentage => Math.Round(((Last - SodUtc8) / SodUtc8) * 100, 2);

    // 均价
    public double Avg => VolCcy24h / Vol24h;
}