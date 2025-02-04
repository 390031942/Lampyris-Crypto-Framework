namespace Lampyris.Server.Crypto.Common;

public static class RealTimeQuoteService
{
    private static Dictionary<string, QuoteTickerData> ms_RealTimeQuoteDataMap = new ();

    private static long ms_LastestTimestamp = DateTimeUtil.GetCurrentTimestamp();

    private static List<QuoteTickerData> ms_RealTimeQuoteDataList = new ();
    public static IReadOnlyCollection<QuoteTickerData> TickQuote(InstType instType)
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