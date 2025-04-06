namespace Lampyris.Server.Crypto.Common;

/*
 * 被DBColumnAttribute标记的类可以作为数据库表格中的一列
 */
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DBColumnAttribute : Attribute
{
    public string ColumnName { get; }
    public string DataType { get; }
    public bool IsPrimaryKey { get; }
    public bool IsAutoIncrement { get; }

    public bool IsNotNull { get; }

    public DBColumnAttribute(string columnName, string dataType, bool isPrimaryKey = false, bool isAutoIncrement = false, bool isNotNull = false)
    {
        ColumnName = columnName;
        DataType = dataType;
        IsPrimaryKey = isPrimaryKey;
        IsAutoIncrement = isAutoIncrement;
        IsNotNull = isNotNull;
    }
}
