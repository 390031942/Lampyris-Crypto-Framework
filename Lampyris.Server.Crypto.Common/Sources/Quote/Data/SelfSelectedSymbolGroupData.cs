namespace Lampyris.Server.Crypto.Common;

[DBTable("self_selected_symbol")]

public class SelfSelectedSymbolGroupData
{
    [DBColumn("clientUserId", "INTEGER", isPrimaryKey: true)]
    public int ClientUserId;

    // 自选组名称
    [DBColumn("name", "DATETIME", isPrimaryKey: true)]
    public string Name;

    // 交易对列表
    [DBColumn("symbolList", "JSON")]
    public List<SelfSelectedSymbolInfoData> SymbolList = new List<SelfSelectedSymbolInfoData>();

    // 是否是动态分组，如果是则不可删除
    public bool IsDynamicGroup;
}
