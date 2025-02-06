namespace Lampyris.Server.Crypto.Common;

using System;
using System.Collections.Generic;

public static class HistoricalQuoteService
{
    /// <summary>
    /// 查询最近/历史k线的实现(同步)
    /// </summary>
    /// <param name="isHistory">用于区分是查询最近/历史</param>
    /// <param name="instId">产品ID，如 BTC-USDT</param>
    /// <param name="barSize">时间粒度，默认值1m</param>
    /// <param name="after">请求此时间戳之前（更旧的数据）的分页内容</param>
    /// <param name="before">请求此时间戳之后（更新的数据）的分页内容， 单独使用时，会返回最新的数据。</param>
    /// <param name="limit">分页返回的结果集数量</param>
    private static List<QuoteCandleData> QueryCandleImpl(bool isHistory, string instId, BarSize barSize, DateTime? after,DateTime? before,int? limit)
    {
        string url = OkxRequestUrlParamMaker.GetCandleUrl(isHistory,instId,barSize, after,before,limit);

        List<QuoteCandleData> result = new List<QuoteCandleData>();
        HttpRequest.GetSync(url, (json =>
        {
            try
            {
                OkxResponseJsonParser.ParseCandleListNoAlloc(json, result);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"Parsing json failed, reason: {ex.Message}");
            }
        }));

        return result; 
    }

    private static WaitForQuoteCandleResult QueryCandleAsyncImpl(bool isHistory, string instId, BarSize barSize, DateTime? after, DateTime? before, int? limit)
    {
        string url = OkxRequestUrlParamMaker.GetCandleUrl(isHistory, instId, barSize, after, before, limit);
        WaitForQuoteCandleResult waitForQuoteCandleResult = new WaitForQuoteCandleResult(url);
        return waitForQuoteCandleResult;
    }

    public static List<QuoteCandleData> QueryRecentCandle(string instId, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 300)
    {
        return QueryCandleImpl(false, instId, barSize, after, before, limit);
    }

    public static List<QuoteCandleData> QueryHistoryCandle(string instId, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 100)
    {
        return QueryCandleImpl(true, instId, barSize, after, before, limit);
    }

    public static WaitForQuoteCandleResult QueryRecentCandleAsync(string instId, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 300)
    {
        return QueryCandleAsyncImpl(false, instId, barSize, after, before, limit);
    }

    public static WaitForQuoteCandleResult QueryHistoryCandleAsync(string instId, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 100)
    {
        return QueryCandleAsyncImpl(true, instId, barSize, after, before, limit);
    }
}