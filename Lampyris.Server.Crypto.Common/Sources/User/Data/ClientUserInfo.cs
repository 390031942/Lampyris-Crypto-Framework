namespace Lampyris.Server.Crypto.Common;

[DBTable("user_info")]
public class ClientUserInfo
{
    [DBColumn("userId", "INTERGER",isPrimaryKey: true,isAutoIncrement:true)] // 用户ID
    public int      UserId;

    [DBColumn("userName", "STRING")] // 用户名
    public string   UserName;

    [DBColumn("headIcon", "STRING")] // 头像文件路径
    public string   HeadIcon;

    [DBColumn("deviceName", "STRING")] // 设备名称
    public string   DeviceName;

    [DBColumn("macAddress", "STRING")] // 设备MAC地址
    public string   MacAddress;

    [DBColumn("lastOnline", "DATETIME")] // 最后在线时间
    public DateTime LastOnline;
}
