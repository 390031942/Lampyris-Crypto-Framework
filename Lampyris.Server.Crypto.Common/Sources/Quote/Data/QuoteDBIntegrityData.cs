namespace Lampyris.Server.Crypto.Common;

public class QuoteDBIntegrityData
{
    public Dictionary<string, PerSymbolIntegrityData> SymbolIntegrityDataMap = new ();

    public class PerSymbolIntegrityData
    {
        public DateTime StartDate;
        public DateTime EndDate;
    }
}
