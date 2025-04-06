namespace Lampyris.Server.Crypto.Common;

public class SelfSelectedSymbolGroup
{
    // ��ѡ������
    public string GroupName;

    // �Ƿ����ɾ��
    public bool CanDelete;
}

[DBTable("self_selected_symbol_group")]
public class SelfSelectedSymbolGroupList
{
    [DBColumn("id", "INTEGER", isPrimaryKey: true)] // �û�ID������
    public string UserId { get; set; }

    [DBColumn("group_data_list", "JSON", isPrimaryKey: true)] // �����б�
    public List<SelfSelectedSymbolGroup> GroupList { get; set; }
}
