namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

[Component]
public class AbstractRealTimeQuoteService
{
    private Dictionary<string, QuoteTickerData> m_RealTimeQuoteDataMap = new ();

    private long m_LastestTimestamp = Lampyris.CSharp.Common.DateTimeUtil.GetCurrentTimestamp();

    private List<QuoteTickerData> m_RealTimeQuoteDataList = new ();

    public IReadOnlyCollection<QuoteTickerData> TickQuote()
    {
        string url = NetworkConfig.BaseUrl + $"/api/v5/market/tickers?instType={instType}";
        HttpRequest.GetSync(url,(json =>
        {
            try
            {
                OkxResponseJsonParser.ParseTickerListNoAlloc(json,ms_RealTimeQuoteDataList);
                foreach (QuoteTickerData quoteTickerData in ms_RealTimeQuoteDataList)
                {
                    QuoteCacheService.Instance.StorageInstId(instType, quoteTickerData.InstId);
                    ms_LastestTimestamp = quoteTickerData.Ts;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"Parsing json failed, reason: {ex.Message}");
            }
        }));

        ms_RealTimeQuoteDataMap.Clear();
        if (ms_RealTimeQuoteDataList != null)
        {
            foreach (QuoteTickerData data in ms_RealTimeQuoteDataList)
            {
                ms_RealTimeQuoteDataMap[data.InstId] = data;
            }
            return ms_RealTimeQuoteDataList.AsReadOnly();
        }

        return null;
    }

    public static QuoteTickerData Query(string instId)
    {
        if (!ms_RealTimeQuoteDataMap.ContainsKey(instId))
            return null;

        return ms_RealTimeQuoteDataMap[instId];
    }

    public static DateTime QueryLatestDateTime()
    {
        return DateTimeUtil.FromUnixTimestamp(ms_LastestTimestamp);
    }
}