namespace Lampyris.Server.Crypto.Common;

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public class DBColumnProperty
{
    public string            ColumnName;
    public PropertyInfo      PropertyInfo;
    public DBColumnAttribute Attribute;
};

public class DBTable<T>
{
    private readonly MySqlConnection               m_Connection;
    private readonly string                        m_TableName;
    private readonly string                        m_dbInsertString;
    private static readonly List<DBColumnProperty> ms_ColumnList = new List<DBColumnProperty>();

    static DBTable()
    {
        foreach (var property in typeof(T).GetProperties())
        {
            var columnAttribute = property.GetCustomAttribute<DBColumnAttribute>();
            if (columnAttribute != null && !columnAttribute.IsAutoIncrement)
            {
                ms_ColumnList.Add(new DBColumnProperty() 
                {
                    ColumnName   = columnAttribute.ColumnName,
                    PropertyInfo = property,
                    Attribute    = columnAttribute,
                });
            }
        }
    }

    public DBTable(string tableName, MySqlConnection connection)
    {
        m_TableName  = tableName;
        m_Connection = connection;

        StringBuilder sb = new StringBuilder();
        sb.Append($"INSERT INTO {m_TableName} (");
        sb.Append(string.Join(", ", ms_ColumnList.Select(c => c.ColumnName)));
        sb.Append(") VALUES {0};");

        m_dbInsertString = sb.ToString();
    }

    public void Insert(T entity)
    {
        var values = new List<string>();

        StringBuilder sb = new StringBuilder();
        AppendSingleEntity(entity, sb);

        var insertSql = string.Format(m_dbInsertString, sb.ToString());
        using (var command = new MySqlCommand(insertSql, m_Connection))
        {
            command.ExecuteNonQuery();
        }

        Console.WriteLine($"Data inserted into table '{m_TableName}' successfully.");
    }

    private static void AppendSingleEntity(T entity, StringBuilder sb)
    {
        sb.Append("(");
        bool isFirst = true;
        foreach (var property in ms_ColumnList)
        {
            var columnAttribute = property.Attribute;
            if (columnAttribute != null && !columnAttribute.IsAutoIncrement)
            {
                var value = property.PropertyInfo.GetValue(entity);
                var valueString = (value == null ? "NULL" : $"'{value.ToString().Replace("'", "''")}'");

                sb.Append(isFirst ? "" : ",");
                sb.Append(valueString);

                isFirst = false;
            }
        }
        sb.Append(")");
    }

    public void Insert(IList<T> entities)
    {
        if (entities == null || entities.Count == 0)
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < entities.Count; i++)
        {
            T entity = entities[i];
            AppendSingleEntity(entity, sb);
            if(i < entities.Count - 1)
            {
                sb.Append(",");
            }
        }

        // 拼接批量插入的 SQL
        var insertSql = string.Format(m_dbInsertString, sb.ToString());
        // 执行 SQL
        using (var transaction = m_Connection.BeginTransaction())
        {
            using (var command = new MySqlCommand(insertSql, m_Connection, transaction))
            {
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }

        Console.WriteLine($"Batch data inserted into table '{m_TableName}' successfully.");
    }
}
