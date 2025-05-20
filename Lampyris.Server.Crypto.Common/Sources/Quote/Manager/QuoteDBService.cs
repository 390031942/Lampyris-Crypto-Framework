using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component("QuoteDBService")]
public class QuoteDBService : DBService
{
    public override string DatebaseName => "lampyris.crpyto.db.quote";
}