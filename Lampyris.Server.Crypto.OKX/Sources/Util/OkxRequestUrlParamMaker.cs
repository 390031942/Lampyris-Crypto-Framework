namespace Lampyris.Server.Crypto.Common;

using System;

public static class OkxRequestUrlParamMaker
{
    public static string GetCandleUrl(bool isHistory, string symbol, BarSize barSize, DateTime? after, DateTime? before, int? limit)
    {
        string url = NetworkConfig.BaseUrl + $"/api/v5/market/{(isHistory ? "history-candles" : "candles")}?symbol={symbol}";

        // barSize
        url += $"&bar={EnumNameManager.GetName(barSize)}";

        // after
        if (after != null)
        {
            url += $"&$after={DateTimeUtil.ToUnixTimestampMilliseconds(after.Value)}";
        }

        // before
        if (before != null)
        {
            url += $"&before={DateTimeUtil.ToUnixTimestampMilliseconds(before.Value)}";
        }

        // limit
        if (limit != null)
        {
            url += $"&limit={limit}";
        }

        return url;
    }

}
