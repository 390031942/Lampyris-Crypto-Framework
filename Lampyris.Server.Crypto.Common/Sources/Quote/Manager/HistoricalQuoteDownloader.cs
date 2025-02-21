namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.Collections;

[Component]
public class HistoricalQuoteDownloader
{
    [Autowired]
    private AbstractRealTimeQuoteService m_RealTimeQuoteService;

    [Autowired]
    private QuoteCacheService m_QuoteCacheService;

    public void DownloadSingleCandle(string symbol)
    {

    }

    public void DownloadCandles(List<string> symbolList)
    {

    }

    public void DownloadAllRecentCandle(List<BarSize> barSizes, Action<BarSize>? callback = null)
    {
        IReadOnlyCollection<QuoteTickerData> dataList = m_RealTimeQuoteService.GetTickerDataList();
        List<string> symbolList = new List<string>();
        foreach (QuoteTickerData data in dataList)
        {
            symbolList.Add(data.Symbol);
        }
        foreach(BarSize barSize in barSizes)
        {
            CoroutineManager.StartCoroutine(DownloadRecentCandleProcess(barSize, callback, 0.05 * barSizes.Count));
        }
    }

    public void DownloadAllHistoryCandle(List<BarSize> barSizes, int n, Action<BarSize>? callback = null)
    {
        IReadOnlyCollection<QuoteTickerData> dataList = RealTimeQuoteService.TickQuote(okxInstType);
        List<string> symbolList = new List<string>();
        foreach (QuoteTickerData data in dataList)
        {
            symbolList.Add(data.Symbol);
        }
        foreach (BarSize barSize in barSizes)
        {
            CoroutineManager.StartCoroutine(DownloadHistoryCandleProcess(symbolList, barSize, n, callback, 0.05 * barSizes.Count));
        }
    }

    private IEnumerator DownloadRecentCandleProcess(List<string> symbolList, BarSize okxBarSize, Action<BarSize>? callback, double delaySec = 0.1)
    {
        Logger.LogInfo($"Start to download recent candle, okxBarSize = {okxBarSize}");
        int progress = 0;
        foreach (string symbol in symbolList)
        {
            QuoteCandleData lastestCandleData = QuoteCacheService.Instance.QueryLastest(symbol, okxBarSize);
            var canldeFuture =.QueryRecentCandleAsync(symbol, okxBarSize);
            yield return canldeFuture;

            QuoteCacheService.Instance.Storage(symbol, okxBarSize, canldeFuture.GetResult());

            yield return new WaitForSeconds(delaySec);

            Logger.LogInfo($"Downloading recent candle, okxBarSize = {okxBarSize}, process = {++progress}/{symbolList.Count}");
        }

        Logger.LogInfo($"Downloading recent candle, okxBarSize = {okxBarSize} Finished!");
        callback?.Invoke(okxBarSize);
    }

    private IEnumerator DownloadHistoryCandleProcess(List<string> symbolList, BarSize okxBarSize, int n, Action<BarSize>? callback, double delaySec = 0.1)
    {
        Logger.LogInfo($"Start to download historical candle, okxBarSize = {okxBarSize}");
        int progress = 0;
        foreach (string symbol in symbolList)
        {
            QuoteCandleData lastestCandleData = m_QuoteCacheService.QueryLastest(symbol, okxBarSize);
            var canldeFuture = QueryHistoryCandleAsync(symbol, okxBarSize,limit:n);
            yield return canldeFuture;

            m_QuoteCacheService.Storage(symbol, okxBarSize, canldeFuture.GetResult());
            yield return new WaitForSeconds(delaySec);

            Logger.LogInfo($"Downloading historical candle, okxBarSize = {okxBarSize}, process = {++progress}/{symbolList.Count}");
        }

        Logger.LogInfo($"Downloading historical candle, okxBarSize = {okxBarSize} Finished!");
        callback?.Invoke(okxBarSize);
    }
}