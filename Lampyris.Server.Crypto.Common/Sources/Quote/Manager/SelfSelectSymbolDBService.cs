using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component("SelfSelectSymbolDBService")]
public class SelfSelectSymbolDBService : DBService
{
    public override string DatebaseName => "lampyris.crpyto.db.selfselect";
}
