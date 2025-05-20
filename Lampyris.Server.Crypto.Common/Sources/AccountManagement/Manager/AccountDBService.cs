using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component("AccountDBService")]
public class AccountDBService : DBService
{
    public override string DatebaseName => "lampyris.crpyto.db.account";
}