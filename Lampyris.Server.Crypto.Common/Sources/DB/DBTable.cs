namespace Lampyris.Server.Crypto.Common;

using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
    private readonly string                        m_dbInsertStringWithUpdate;
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
        m_TableName = tableName;
        m_Connection = connection;

        // 拼接字段列表
        string fieldString = string.Join(", ", ms_ColumnList.Select(c => c.ColumnName));

        // 构建 INSERT INTO 部分（无更新）
        StringBuilder sbInsert = new StringBuilder();
        sbInsert.Append($"INSERT INTO {m_TableName} (");
        sbInsert.Append(fieldString);
        sbInsert.Append(") VALUES {0};");
        m_dbInsertString = sbInsert.ToString();

        // 构建 INSERT INTO 部分（带更新）
        StringBuilder sbInsertWithUpdate = new StringBuilder(sbInsert.ToString().TrimEnd(';')); // 去掉末尾的分号
        if (ms_ColumnList.Any(c => !c.Attribute.IsAutoIncrement))
        {
            sbInsertWithUpdate.Append(" ON DUPLICATE KEY UPDATE ");
            sbInsertWithUpdate.Append(string.Join(", ", ms_ColumnList
                .Where(c => !c.Attribute.IsAutoIncrement) // 排除自增列
                .Select(c => $"{c.ColumnName} = VALUES({c.ColumnName})")));
        }
        sbInsertWithUpdate.Append(";");
        m_dbInsertStringWithUpdate = sbInsertWithUpdate.ToString();

        // 构建 SELECT 查询的字段列表
        m_dbQueryString = $"SELECT {fieldString} FROM {m_TableName}";
    }

    public void Insert(T entity, bool update = false)
    {
        StringBuilder sb = new StringBuilder();
        AppendSingleEntity(entity, sb);

        // 根据 update 参数选择 SQL 模板
        var insertSql = update
            ? string.Format(m_dbInsertStringWithUpdate, sb.ToString())
            : string.Format(m_dbInsertString, sb.ToString());

        using (var command = new MySqlCommand(insertSql, m_Connection))
        {
            command.ExecuteNonQuery();
        }
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
                var valueString = "";

                if (property.Attribute.DataType == "JSON") {
                    valueString = JsonConvert.SerializeObject(value);
                }
                else {
                    valueString = (value == null ? "NULL" : $"'{value.ToString().Replace("'", "''")}'");
                }

                sb.Append(isFirst ? "" : ",");
                sb.Append(valueString);

                isFirst = false;
            }
        }
        sb.Append(")");
    }

    public void Insert(IList<T> entities, bool update = false)
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
            if (i < entities.Count - 1)
            {
                sb.Append(",");
            }
        }

        // 根据 update 参数选择 SQL 模板
        var insertSql = update
            ? string.Format(m_dbInsertStringWithUpdate, sb.ToString())
            : string.Format(m_dbInsertString, sb.ToString());

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

    public List<T> Query(string queryCondition = "", KeyValuePair<string,object>[]? parameters = null, string orderBy = "", bool ascending = true)
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
                            if (column.Attribute.DataType == "JSON") // Json
                            {
                                string json = reader.GetString(reader.GetOrdinal(columnName));
                                object? obj = JsonConvert.DeserializeObject(json, column.PropertyInfo.PropertyType);
                                property.SetValue(entity, obj);
                            }
                            else
                            {
                                var value = reader[columnName];
                                property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                    }

                    result.Add(entity);
                }
            }
        }
        return result;
    }
}
