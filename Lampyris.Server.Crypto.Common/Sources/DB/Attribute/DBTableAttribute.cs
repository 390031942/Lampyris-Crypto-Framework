namespace Lampyris.Server.Crypto.Common;

/*
 * 被DBTableAttribute标记的类可以作为数据库实体，从而写入数据库
 */
[AttributeUsage(AttributeTargets.Class)]
public class DBTableAttribute:Attribute
{
    /*
     * 表示数据库表格名称，如"tableName_{0}_{1}",
     * 在DBService里可以传入对应参数以替换占位符，从而得到存储于数据库的表格名 
     */
    public string TableNameTemplate { get; }

    public DBTableAttribute(string tableNameTemplate)
    {
        TableNameTemplate = tableNameTemplate;
    }

    public string GetTableName(params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            return TableNameTemplate;
        }

        return string.Format(TableNameTemplate, args);
    }
}
