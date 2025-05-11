namespace Lampyris.Server.Crypto.Common;

[DBTable("self_selected_symbol")]
public class SelfSelectedSymbolData
{
    [DBColumn("id", "INTEGER", isPrimaryKey: true)] // �û�ID������1
    public string UserId { get; set; }

    [DBColumn("group_name", "STRING", isPrimaryKey: true)] // ����������2
    public string GroupName { get; set; }

    [DBColumn("Symbol", "STRING", isPrimaryKey: true)] // ���׶ԣ�����
    public string Symbol { get; set; }

    [DBColumn("Timestamp", "BIGINT")] // ��ѡʱ���
    public long Timestamp { get; set; }

    [DBColumn("initial_price", "DOUBLE")] // ��ѡ�۸�
    public double InitialPrice { get; set; }
}
