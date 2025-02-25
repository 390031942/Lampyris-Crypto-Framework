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

public class SQLParamMaker
{
    private List<KeyValuePair<string, object>> m_ResultList = new List<KeyValuePair<string, object>>();

    public static SQLParamMaker Begin()
    {
        return new SQLParamMaker();
    }

    public SQLParamMaker Append(string key, object value)
    {
        m_ResultList.Add(new KeyValuePair<string, object>(key, value));
        return this;
    }

    public KeyValuePair<string, object>[] End()
    {
        return m_ResultList.ToArray();
    }
}

public class DBTable<T> where T:class, new()
{
    private readonly MySqlConnection               m_Connection;
    private readonly string                        m_TableName;
    private readonly string                        m_dbInsertString;
    private readonly string                        m_dbQueryString;
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

        string fieldString = string.Join(", ", ms_ColumnList.Select(c => c.ColumnName));
        StringBuilder sb = new StringBuilder();
        sb.Append($"INSERT INTO {m_TableName} (");
        sb.Append(fieldString);
        sb.Append(") VALUES {0};");

        m_dbInsertString = sb.ToString();
        m_dbQueryString =  $"SELECT {fieldString} FROM {m_TableName}";
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

    public List<T> Query(string queryCondition, KeyValuePair<string,object>[]? parameters = null, string orderBy = "", bool ascending = true)
    {
        List<T> result = new List<T>();

        // 构建查询 SQL
        StringBuilder sb = new StringBuilder();
        sb.Append(m_dbQueryString);

        // 添加查询条件
        if (!string.IsNullOrEmpty(queryCondition))
        {
            sb.Append($" WHERE {queryCondition}");
        }

        // 添加排序条件
        if (!string.IsNullOrEmpty(orderBy))
        {
            sb.Append($" ORDER BY {orderBy} {(ascending ? "ASC" : "DESC")}");
        }

        string querySql = sb.ToString();

        // 执行查询
        using (var command = new MySqlCommand(querySql, m_Connection))
        {
            if (parameters != null)
            {
                foreach (var pair in parameters)
                {
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                }
            }
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // 创建实体对象
                    T entity = new T();

                    // 遍历列并赋值到实体对象
                    foreach (var column in ms_ColumnList)
                    {
                        var columnName = column.ColumnName;
                        var property = column.PropertyInfo;

                        if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                        {
                            var value = reader[columnName];
                            property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
                        }
                    }

                    result.Add(entity);
                }
            }
        }

        return result;
    }

}
