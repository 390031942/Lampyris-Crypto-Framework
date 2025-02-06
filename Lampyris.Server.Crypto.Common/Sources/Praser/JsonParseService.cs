namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

[Component]
public abstract class AbstractJsonParseService
{
    /*
     * 解析全体行情列表
     */
    public abstract List<QuoteTickerData> parseCandleTickerData(string json, List<QuoteTickerData>? allocatedList = null);

    /*
     * 解析返回k线数据列表 
    */
    public abstract List<QuoteCandleData> parseCandleData(string json, List<QuoteCandleData>? allocatedList = null);
}
