namespace Lampyris.Server.Crypto.Common;

public class SelfSelectedSymbolGroup
{
    // 自选组名称
    public string GroupName;

    // 是否可以删除
    public bool CanDelete;
}

[DBTable("self_selected_symbol_group")]
public class SelfSelectedSymbolGroupList
{
    [DBColumn("id", "INTEGER", isPrimaryKey: true)] // 用户ID，主键
    public string UserId { get; set; }

    [DBColumn("group_data_list", "JSON", isPrimaryKey: true)] // 组名列表
    public List<SelfSelectedSymbolGroup> GroupList { get; set; }
}
