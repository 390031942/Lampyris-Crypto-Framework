namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System;
using System.Collections.Generic;

[Component]
public class HistoricalQuoteService
{
    /// <summary>
    /// 查询最近/历史k线的实现(同步)
    /// </summary>
    /// <param name="isHistory">用于区分是查询最近/历史</param>
    /// <param name="symbol">产品ID，如 BTC-USDT(不同的交易所的symbol格式不同)</param>
    /// <param name="barSize">时间粒度，默认值1m</param>
    /// <param name="after">请求此时间戳之前（更旧的数据）的分页内容</param>
    /// <param name="before">请求此时间戳之后（更新的数据）的分页内容， 单独使用时，会返回最新的数据。</param>
    /// <param name="limit">分页返回的结果集数量</param>
    private List<QuoteCandleData> QueryCandleImpl(bool isHistory, string symbol, BarSize barSize, DateTime? after,DateTime? before,int? limit)
    {
        return null;
    }

    private WaitForQuoteCandleResult QueryCandleAsyncImpl(bool isHistory, string symbol, BarSize barSize, DateTime? after, DateTime? before, int? limit)
    {
        return null;
    }

    public List<QuoteCandleData> QueryRecentCandle(string symbol, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 300)
    {
        return QueryCandleImpl(false, symbol, barSize, after, before, limit);
    }

    public List<QuoteCandleData> QueryHistoryCandle(string symbol, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 100)
    {
        return QueryCandleImpl(true, symbol, barSize, after, before, limit);
    }

    public WaitForQuoteCandleResult QueryRecentCandleAsync(string symbol, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 300)
    {
        return QueryCandleAsyncImpl(false, symbol, barSize, after, before, limit);
    }

    public WaitForQuoteCandleResult QueryHistoryCandleAsync(string symbol, BarSize barSize = BarSize._1m, DateTime? after = null, DateTime? before = null, int? limit = 100)
    {
        return QueryCandleAsyncImpl(true, symbol, barSize, after, before, limit);
    }
}