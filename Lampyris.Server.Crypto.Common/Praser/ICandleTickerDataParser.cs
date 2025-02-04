namespace Lampyris.Server.Crypto.Common;
public interface ICandleTickerDataParser
{
    public List<QuoteCandleData> parse(string json);
}
