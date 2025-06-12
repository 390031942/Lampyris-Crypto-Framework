namespace Lampyris.Server.Crypto.Common;

// 涨跌分布区域数据
public class MarketPreviewIntervalData
{
    public int LowerBoundPerc; // 下界(%)
    public int UpperBoundPerc; // 上界(%)
    public int Count;          // 数量(家)
};

public class MarketSummaryData
{
    // USDT永续合约 总数
    public int ContractCount => RiseCount + FallCount + UnchangedCount;

    // 涨跌平数量
    public int RiseCount;
    public int FallCount;
    public int UnchangedCount;

    // 平均涨跌幅(相对时区 UTC+0)
    public decimal AvgChangePerc;

    // 前10名平均涨跌幅(相对时区 UTC+0)
    public decimal Top10AvgChangePerc;
    public decimal Last10AvgChangePerc;

    // 主流币平均涨跌幅
    public decimal MainStreamAvgChangePerc;

    public List<MarketPreviewIntervalData> IntervalDatas = new List<MarketPreviewIntervalData>();

    public void Reset()
    {
        RiseCount = 0;
        FallCount = 0;
        UnchangedCount = 0;
        AvgChangePerc = 0;
        Top10AvgChangePerc = 0;
        Last10AvgChangePerc = 0;
        Last10AvgChangePerc = 0;
        MainStreamAvgChangePerc = 0;

        if (IntervalDatas.Count <= 0)
        {
            // IntervalDatas需要保存
            // (-∞,-10%],(-10%,9%],(-9%,8%] .. (-1%,0%] .. 0% .. (0%,1] .. [9%,10%),[10%,+∞)
            // 这21个区间的涨跌幅个数


            // (-∞,-10%] 跌幅大于等于10%
            IntervalDatas.AddRange(new MarketPreviewIntervalData[21]);
            IntervalDatas[0] = new MarketPreviewIntervalData();
            IntervalDatas[0].LowerBoundPerc = -99999999;
            IntervalDatas[0].UpperBoundPerc = -10;

            // 0% 平盘的
            IntervalDatas[10] = new MarketPreviewIntervalData();
            IntervalDatas[10].LowerBoundPerc = 0;
            IntervalDatas[10].UpperBoundPerc = 0;

            // [10%,+∞) 涨幅大于等于10%
            IntervalDatas[20] = new MarketPreviewIntervalData();
            IntervalDatas[20].LowerBoundPerc = 10;
            IntervalDatas[20].UpperBoundPerc = 99999999;

            for (int i = 1; i <= 19; i++)
            {
                if(i != 10)
                {
                    var intervalData = IntervalDatas[i] = new MarketPreviewIntervalData();
                    intervalData.LowerBoundPerc = i - 10;
                    intervalData.UpperBoundPerc = intervalData.LowerBoundPerc + 1;
                }
            }
        }
    }

    /// <summary>
    /// 记录一个涨跌幅，并存储到对应的涨跌幅分布中
    /// </summary>
    /// <param name="perc"></param>
    public void RecordPerc(decimal perc)
    {
        // 找到对应的区间并增加计数
        foreach (var interval in IntervalDatas)
        {
            if (perc >= interval.LowerBoundPerc && perc < interval.UpperBoundPerc)
            {
                interval.Count++;
                return;
            }
        }

        // 处理边界情况
        if (perc >= 10)
        {
            IntervalDatas[20].Count++;
        }
        else if (perc <= -10)
        {
            IntervalDatas[0].Count++;
        }
        else if (perc == 0)
        {
            IntervalDatas[10].Count++;
        }
    }
}