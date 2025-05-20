using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component]
public class TradeDBService : DBService
{
    public override string DatebaseName => "lampyris.crpyto.db.trade";
}
