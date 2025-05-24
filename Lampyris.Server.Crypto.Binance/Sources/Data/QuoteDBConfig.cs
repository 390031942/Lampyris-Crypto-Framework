using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Binance;

[IniFile("quote_db.ini")]
public class QuoteDBConfig
{
    [IniField]
    public string DirecotryPath;
}
