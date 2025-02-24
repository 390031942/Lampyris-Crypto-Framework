namespace Lampyris.Server.Crypto.Common;

public class MarketSummaryData
{
    // USDT永续合约 总数
    public int ContractCount => RiseCount + FallCount + UnchangedCount;

    // 涨跌平数量
    public int RiseCount;
    public int FallCount;
    public int UnchangedCount;

    // 平均涨跌幅(相对时区 UTC+0)
    public double AvgChangePerc;

    // 前10名平均涨跌幅(相对时区 UTC+0)
    public double Top10AvgChangePerc;
    public double Last10AvgChangePerc;
}