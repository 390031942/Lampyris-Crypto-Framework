using Lampyris.Server.Crypto.Common;

namespace Lampyris.Server.Crypto.Binance;

public static class StringUtil
{
    public static string ToParamString(this BarSize barSize)
    {
        return barSize.ToString().Replace("_", "");
    }
}
