using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component("UserDBService")]
public class UserDBService:DBService
{
    public override string DatebaseName => "lampyris.server.crypto.user";

    public ClientUserInfo QueryClientUserByDeviceMAC(string deviceMAC)
    {
        var dbTable = GetTable<ClientUserInfo>();
        var dbData = dbTable.Query(queryCondition: "deviceMAC == @DeviceMAC",
                                   parameters: SQLParamMaker.Begin()
                                                            .Append("DeviceMAC", deviceMAC)
                                                            .End());
        return dbData.Count > 0 ? dbData[0] : null;
    }
}
