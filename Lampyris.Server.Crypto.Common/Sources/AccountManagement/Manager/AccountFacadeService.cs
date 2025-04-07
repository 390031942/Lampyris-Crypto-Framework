using Lampyris.Crypto.Protocol.Common;
using Lampyris.CSharp.Common;
using Lampyris.Crypto.Protocol.Account;

namespace Lampyris.Server.Crypto.Common;

[Component]

public class AccountFacadeService
{
    [MessageHandler(Request.RequestTypeOneofCase.ReqAccountSummaryUpdate)]
    public void ReqAccountSummaryUpdate(ClientUserInfo clientUserInfo, Request request)
    {
        ReqAccountSummaryUpdate req = request.ReqAccountSummaryUpdate;
    }

    [MessageHandler(Request.RequestTypeOneofCase.ReqAccountAssetTransfer)]
    public void ReqAccountAssetTransfer(ClientUserInfo clientUserInfo, Request request)
    {
        ReqAccountAssetTransfer req = request.ReqAccountAssetTransfer;
    }

    [MessageHandler(Request.RequestTypeOneofCase.ReqQueryAssetTransferHistory)]
    public void ReqQueryAssetTransferHistory(ClientUserInfo clientUserInfo, Request request)
    {
        ReqQueryAssetTransferHistory req = request.ReqQueryAssetTransferHistory;
    }
}
