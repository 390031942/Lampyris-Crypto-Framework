namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using DateTimeUtilEx = Common.DateTimeUtilEx;

[Component]
public class MarketMonitorService
{
    [Autowired]
    private MarketDataService m_MarketDataService;

    private readonly static MarketMonitorSetting m_settings = IniConfigManager.Load<MarketMonitorSetting>();

    // 每个symbol对应的异动信息
    protected class PerSymbolActiveInfo
    {
        // 新高新低 部分
        public double   HighPerc;
        public double   LowPerc;
        public DateTime HighLowTimestamp;

        // 1min 上升/下降通道
        public DateTime OneMinKTrendTimestamp;

        // 1min 连红连绿
        public DateTime OneMinContinuousColorTimestamp;

        // 15min 连红连绿
        public DateTime FifteenMinContinuousColorTimestamp;

        // 涨速
        public DateTime ChangeSpeedTimestmap;

        // 区间放量
        public DateTime VolumeIncreaseTimestmap;

        // 1min线 脉冲放量
        public DateTime PulseVolumeIncreaseTimestmap;
    }

    protected List<QuoteCandleData>                 m_QuoteCandleDatas = new List<QuoteCandleData>();
                                                    
    protected Func<double, double, bool>            m_Greater = (double lhs, double rhs) => { return lhs > rhs; };
    protected Func<double, double, bool>            m_Lesser  = (double lhs, double rhs) => { return lhs < rhs; };

    protected Dictionary<string, PerSymbolActiveInfo> m_PerInstActiveInfoMap = new();

    public void Reset()
    {
        m_PerInstActiveInfoMap.Clear();
    }


    public void CheckSymbolForEachTicker(QuoteTickerData tickerData, QuoteCacheService quoteCacheService)
    {
        if (tickerData == null)
            return;

        if (!m_PerInstActiveInfoMap.ContainsKey(tickerData.Symbol))
            m_PerInstActiveInfoMap[tickerData.Symbol] = new PerSymbolActiveInfo();

        PerSymbolActiveInfo perSymbolActiveInfo = m_PerInstActiveInfoMap[tickerData.Symbol];

        quoteCacheService.QueryLastestNoAlloc(tickerData.Symbol, BarSize._1m, m_QuoteCandleDatas, 30);
        DateTime now = CSharp.Common.DateTimeUtil.FromUnixTimestamp(tickerData.Timestamp);

        // 采样最近 1min k线, 判断上升/下降通道
        if (m_QuoteCandleDatas.Count >= 10)
        {
            bool isRise = true;
            bool isFall = true;

            // 1min 均线上升通道判定
            if (DateTimeUtilEx.GetBarTimeSpanDiff(perSymbolActiveInfo.OneMinKTrendTimestamp, now, BarSize._1m) > 0)
            {
                for (int i = m_QuoteCandleDatas.Count - m_settings.OneMinMA5Threshold; i < m_QuoteCandleDatas.Count - 2; i++)
                {
                    if (!CompareMovingAverage(m_QuoteCandleDatas[i - 1], m_QuoteCandleDatas[i], m_Lesser))
                    {
                        isRise = false;
                        break;
                    }
                }
                if (!isRise)
                {
                    // 1min 均线下降通道判定
                    for (int i = m_QuoteCandleDatas.Count - m_settings.OneMinMA5Threshold; i < m_QuoteCandleDatas.Count - 2; i++)
                    {
                        if (!CompareMovingAverage(m_QuoteCandleDatas[i - 1], m_QuoteCandleDatas[i], m_Greater))
                        {
                            isFall = false;
                            break;
                        }
                    }
                }

                if (isRise)
                {
                    Logger.LogInfo($"[异动提示]:{symbol} 1分均线上升通道");
                    perSymbolActiveInfo.OneMinKTrendTimestamp = now;
                }
                else if (isFall)
                {
                    Logger.LogInfo($"[异动提示]:{symbol} 1分均线下降通道");
                    perSymbolActiveInfo.OneMinKTrendTimestamp = now;
                }
            }
        }

        // 采样最近 1min k线, 判断连红,连绿
        if (DateTimeUtilEx.GetBarTimeSpanDiff(perSymbolActiveInfo.OneMinContinuousColorTimestamp, now, BarSize._1m) > 0)
        {
            if (m_QuoteCandleDatas.Count >= m_settings.OneMinSameColorCandleThreshold)
            {
                bool MA5candleContinuousRiseUp = true;
                bool MA5candleContinuousRiseDown = true;

                for (int i = m_QuoteCandleDatas.Count - m_settings.OneMinSameColorCandleThreshold; i < m_QuoteCandleDatas.Count - 2; i++)
                {
                    if (m_QuoteCandleDatas[i - 1].Close >= m_QuoteCandleDatas[i].Close)
                    {
                        MA5candleContinuousRiseUp = false;
                        break;
                    }
                }

                for (int i = m_QuoteCandleDatas.Count - m_settings.OneMinSameColorCandleThreshold; i < m_QuoteCandleDatas.Count - 2; i++)
                {
                    if (m_QuoteCandleDatas[i - 1].Close <= m_QuoteCandleDatas[i].Close)
                    {
                        MA5candleContinuousRiseDown = false;
                        break;
                    }
                }

                if (MA5candleContinuousRiseUp)
                {
                    Logger.LogInfo($"[异动提示]:{symbol} 1分k线连阳");
                    perSymbolActiveInfo.OneMinContinuousColorTimestamp = now;
                }
                else if (MA5candleContinuousRiseDown)
                {
                    Logger.LogInfo($"[异动提示]:{symbol} 1分k线连阴");
                    perSymbolActiveInfo.OneMinContinuousColorTimestamp = now;
                }
            }
        }

        // 采样最近5根 15min k线, 判断连红连绿+上升通道
        if (DateTimeUtilEx.GetBarTimeSpanDiff(perSymbolActiveInfo.FifteenMinContinuousColorTimestamp, now, BarSize._15m) > 0)
        {
            if (m_QuoteCandleDatas.Count >= 5)
            {
                bool candleContinuousRiseUp = true;
                bool candleContinuousRiseDown = true;

                for (int i = m_QuoteCandleDatas.Count - 5; i < m_QuoteCandleDatas.Count - 2; i++)
                {
                    if (m_QuoteCandleDatas[i - 1].Close < m_QuoteCandleDatas[i].Close)
                    {
                        candleContinuousRiseUp = false;
                        break;
                    }
                }
                for (int i = m_QuoteCandleDatas.Count - 5; i < m_QuoteCandleDatas.Count - 2; i++)
                {
                    if (m_QuoteCandleDatas[i - 1].Close > m_QuoteCandleDatas[i].Close)
                    {
                        candleContinuousRiseDown = false;
                        break;
                    }
                }

                if (candleContinuousRiseUp)
                {
                    Logger.LogInfo($"[异动提示]:{symbol} 15分k线连阳");
                    perSymbolActiveInfo.FifteenMinContinuousColorTimestamp = now;
                }
                else if (candleContinuousRiseDown)
                {
                    Logger.LogInfo($"[异动提示]:{symbol} 15分k线连阴");
                    perSymbolActiveInfo.FifteenMinContinuousColorTimestamp = now;
                }
            }
        }

        // 24小时新高/新低且大于1%
        if (DateTimeUtilEx.GetBarTimeSpanDiff(perSymbolActiveInfo.HighLowTimestamp, now, BarSize._1m) > 0)
        {
            if (m_QuoteCandleDatas.Count >= 10)
            {
                QuoteTickerData realTimeData = RealTimeQuoteService.Query(symbol);
                if (realTimeData != null)
                {
                    PerSymbolActiveInfo newPercentangeInfo = m_PerInstActiveInfoMap.ContainsKey(symbol) ?
                                                           m_PerInstActiveInfoMap[symbol] : new PerSymbolActiveInfo();
                    if (realTimeData.ChangePerc >= 1)
                    {
                        if (realTimeData.ChangePerc > newPercentangeInfo.HighPerc)
                        {
                            perSymbolActiveInfo.HighLowTimestamp = now;
                            Logger.LogInfo($"[异动提示]:{symbol} 24小时内新高，达到{realTimeData.ChangePerc}%");
                        }
                    }
                    else if (realTimeData.ChangePerc <= -1)
                    {
                        if (realTimeData.ChangePerc < newPercentangeInfo.LowPerc)
                        {
                            perSymbolActiveInfo.HighLowTimestamp = now;
                            Logger.LogInfo($"[异动提示]:{symbol} 24小时内新低，达到{realTimeData.ChangePerc}%");
                        }
                    }
                }
            }
        }

        // 分钟级涨速/跌速>1.5%
        if (DateTimeUtilEx.GetBarTimeSpanDiff(perSymbolActiveInfo.ChangeSpeedTimestmap, now, BarSize._1m) > 1)
        {
            if (m_QuoteCandleDatas.Count >= 3)
            {
                QuoteTickerData realTimeData = RealTimeQuoteService.Query(symbol);

                var data1 = m_QuoteCandleDatas[m_QuoteCandleDatas.Count - 1];
                var data2 = m_QuoteCandleDatas[m_QuoteCandleDatas.Count - 3];

                double perc = data1.ChangePercentage(data2);
                double percThreshold = 1.5;

                if (perc > percThreshold)
                {
                    perSymbolActiveInfo.HighLowTimestamp = now;
                    Logger.LogInfo($"[异动提示]:{symbol} 涨速达到{percThreshold}%，{realTimeData.ChangePerc}%");
                }
                else if (perc < -percThreshold)
                {
                    perSymbolActiveInfo.HighLowTimestamp = now;
                    Logger.LogInfo($"[异动提示]:{symbol} 跌速达到{percThreshold}%，{realTimeData.ChangePerc}%");
                }
            }
        }

        // 区间放量
        if (DateTimeUtilEx.GetBarTimeSpanDiff(perSymbolActiveInfo.VolumeIncreaseTimestmap, now, BarSize._1m) > 5)
        {
            if (m_QuoteCandleDatas.Count >= 15)
            {
                double moneyAvg3 = 0.0;
                double moneyAvg15 = 0.0;
                for (int i = 0; i < 15; i++)
                {
                    if (i >= 12)
                    {
                        moneyAvg3 = moneyAvg3 + m_QuoteCandleDatas[m_QuoteCandleDatas.Count - i - 1].Currency;
                    }
                    moneyAvg15 = moneyAvg15 + m_QuoteCandleDatas[m_QuoteCandleDatas.Count - i - 1].Currency;
                }
                moneyAvg3 /= 3;
                moneyAvg15 /= 15;

                if (moneyAvg3 > 5 * moneyAvg15)
                {
                    Logger.LogInfo($"[异动提示]:{symbol} 区间放量{Math.Round(moneyAvg3 / moneyAvg15, 2)}倍");
                    perSymbolActiveInfo.VolumeIncreaseTimestmap = now;
                }
            }
        }

        // 脉冲放量
        bool suddenlyBigVolActive = false;
        if (m_QuoteCandleDatas.Count >= 6)
        {
            double moneySum = 0.0;
            double maxMoney = 0.0;
            double curMinMoney = m_QuoteCandleDatas[m_QuoteCandleDatas.Count - 1].Currency;

            for (int i = 0; i < 5; i++)
            {
                moneySum = moneySum + m_QuoteCandleDatas[m_QuoteCandleDatas.Count - i - 2].Currency;
                maxMoney = Math.Max(maxMoney, moneySum);
            }

            moneySum /= 5;
            if (curMinMoney > 3 * maxMoney & curMinMoney > 3 * moneySum)
            {
                suddenlyBigVolActive = true;
            }
        }
    }
}
