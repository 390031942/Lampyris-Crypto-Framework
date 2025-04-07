namespace Lampyris.Server.Crypto.Common;

[DBTable("sub_trade_account")]
public class SubTradeAccount
{
    [DBColumn("id", "INTEGER", isPrimaryKey: true)]
    public int AccountId = 1; // 子交易账户ID

    [DBColumn("ownerName", "STRING")]
    public string OwnerName = ""; // 账户拥有者名称(便于沟通)

    [DBColumn("ownerEmail", "STRING")]
    public string OwnerEmail = ""; // 账户拥有者邮箱地址

    [DBColumn("ownerPhoneNumber", "STRING")]
    public string OwnerPhoneNumber = ""; // 账户拥有者手机号码

    [DBColumn("isRoot", "STRING")]
    public bool IsRoot = false; // 是否根账户，这涉及到资金的转移

    [DBColumn("apiKey", "STRING")] 
    public string ApiKey    = "";  // API密钥

    [DBColumn("apiSecret", "STRING")]
    public string ApiSecret = ""; // API Secret

    [DBColumn("optional", "STRING")]
    public string Optional  = ""; // 可选参数
}
