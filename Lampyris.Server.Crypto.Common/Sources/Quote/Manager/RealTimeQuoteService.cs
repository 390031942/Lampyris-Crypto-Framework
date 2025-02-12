namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

[Component]
public class AbstractRealTimeQuoteService
{
    protected Dictionary<string, QuoteTickerData> m_RealTimeQuoteDataMap = new ();

    protected List<QuoteTickerData> m_RealTimeQuoteDataList = new ();

    protected Dictionary<string, Dictionary<BarSize, List<QuoteCandleData>>> m_RealTimeCandleDataMap = new();

    public IReadOnlyCollection<QuoteTickerData> GetTickerDataList()
    {
        return m_RealTimeQuoteDataList.AsReadOnly();
    }

    public QuoteTickerData QueryTickerData(string symbol)
    {
        if (!m_RealTimeQuoteDataMap.ContainsKey(symbol))
            return null;

        return m_RealTimeQuoteDataMap[symbol];
    }

    public List<QuoteCandleData> QueryCandleDataList(string symbol, BarSize barSize)
    {
        
    }
}