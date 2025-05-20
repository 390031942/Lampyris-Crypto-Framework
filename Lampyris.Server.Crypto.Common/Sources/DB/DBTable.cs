namespace Lampyris.Server.Crypto.Common;

using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public class DBColumnProperty
{
    public string ColumnName;
    public PropertyInfo PropertyInfo;
    public FieldInfo FieldInfo;
    public DBColumnAttribute Attribute;

    // 检查是否只有一个不为 null
    public bool IsValid()
    {
        return (PropertyInfo != null) ^ (FieldInfo != null); // XOR 确保只有一个不为 null
    }

    // 获取值（根据 PropertyInfo 或 FieldInfo）
    public object? GetValue(object instance)
    {
        if (PropertyInfo != null)
        {
            return PropertyInfo.GetValue(instance);
        }
        else if (FieldInfo != null)
        {
            return FieldInfo.GetValue(instance);
        }
        return null;
    }

    // 设置值（根据 PropertyInfo 或 FieldInfo）
    public void SetValue(object instance, object? value)
    {
        if (PropertyInfo != null)
        {
            PropertyInfo.SetValue(instance, value);
        }
        else if (FieldInfo != null)
        {
            FieldInfo.SetValue(instance, value);
        }
    }
}

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
    private readonly IDBConnectionProvider         m_ConnectionProvider;
    private readonly string                        m_TableName;
    private readonly string                        m_DBInsertString;
    private readonly string                        m_DBInsertStringWithUpdate;
    private readonly string                        m_DBQueryString;
    private static readonly List<DBColumnProperty> ms_ColumnList = new List<DBColumnProperty>();

    static DBTable()
    {
        foreach (var member in typeof(T).GetMembers())
        {
            if (member is PropertyInfo || member is FieldInfo)
            {
                var columnAttribute = member.GetCustomAttribute<DBColumnAttribute>();
                if (columnAttribute != null && !columnAttribute.IsAutoIncrement)
                {
                    ms_ColumnList.Add(new DBColumnProperty()
                    {
                        ColumnName = columnAttribute.ColumnName,
                        PropertyInfo = member is PropertyInfo ? member as PropertyInfo : null,
                        FieldInfo    = member is FieldInfo ? member as FieldInfo : null,
                        Attribute = columnAttribute,
                    });
                }
            }
        }
    }
    public DBTable(string tableName, IDBConnectionProvider connectionProvider)
    {
        m_TableName = tableName;
        m_ConnectionProvider = connectionProvider;

        // 拼接字段列表
        string fieldString = string.Join(", ", ms_ColumnList.Select(c => c.ColumnName));

        // 构建 INSERT INTO 部分（无更新）
        StringBuilder sbInsert = new StringBuilder();
        sbInsert.Append($"INSERT INTO {m_TableName} (");
        sbInsert.Append(fieldString);
        sbInsert.Append(") VALUES {0};");
        m_DBInsertString = sbInsert.ToString();

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
        m_DBInsertStringWithUpdate = sbInsertWithUpdate.ToString();

        // 构建 SELECT 查询的字段列表
        m_DBQueryString = $"SELECT {fieldString} FROM {m_TableName}";
    }

    public void Insert(T entity, bool update = false)
    {
        StringBuilder sb = new StringBuilder();
        AppendSingleEntity(entity, sb);

        // 根据 update 参数选择 SQL 模板
        var insertSql = update
            ? string.Format(m_DBInsertStringWithUpdate, sb.ToString())
            : string.Format(m_DBInsertString, sb.ToString());

        var connection = m_ConnectionProvider.GetConnection();
        using (var command = new MySqlCommand(insertSql, connection))
        {
            command.ExecuteNonQuery();
        }
        m_ConnectionProvider.RecycleConnection(connection);
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
                var value = property.GetValue(entity); // 使用 GetValue 方法
                var valueString = "";

                if (columnAttribute.DataType == "JSON")
                {
                    valueString = JsonConvert.SerializeObject(value);
                }
                else
                {
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
            ? string.Format(m_DBInsertStringWithUpdate, sb.ToString())
            : string.Format(m_DBInsertString, sb.ToString());

        var connection = m_ConnectionProvider.GetConnection();
        using (var transaction = connection.BeginTransaction())
        {
            using (var command = new MySqlCommand(insertSql, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        m_ConnectionProvider.RecycleConnection(connection);
        Console.WriteLine($"Batch data inserted into table '{m_TableName}' successfully.");
    }

    public List<T> Query(string queryCondition = "", KeyValuePair<string,object>[]? parameters = null, string orderBy = "", bool ascending = true)
    {
        List<T> result = new List<T>();

        // 构建查询 SQL
        StringBuilder sb = new StringBuilder();
        sb.Append(m_DBQueryString);

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
        var connection = m_ConnectionProvider.GetConnection();
        using (var command = new MySqlCommand(querySql, connection))
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
                                object? obj = JsonConvert.DeserializeObject(json, column.PropertyInfo?.PropertyType ?? column.FieldInfo?.FieldType);
                                column.SetValue(entity, obj);
                            }
                            else
                            {
                                var value = reader[columnName];
                                column.SetValue(entity, Convert.ChangeType(value, column.PropertyInfo?.PropertyType ?? column.FieldInfo?.FieldType));
                            }
                        }
                    }

                    result.Add(entity);
                }
            }
        }
        m_ConnectionProvider.RecycleConnection(connection);

        return result;
    }

    public IEnumerable<T1> QueryField<T1>(string fieldName, string? whereClause = null, params MySqlParameter[] parameters)
    {
        string query = $"SELECT {fieldName} FROM {m_TableName}";
        if (!string.IsNullOrEmpty(whereClause))
        {
            query += $" WHERE {whereClause}";
        }

        var connection = m_ConnectionProvider.GetConnection();
        try
        {
            using (var command = new MySqlCommand(query, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return reader.IsDBNull(0) ? default : (T1)Convert.ChangeType(reader.GetValue(0), typeof(T1));
                    }
                }
            }
        }
        finally
        {
            m_ConnectionProvider.RecycleConnection(connection);
        }
    }

    public IEnumerable<(T1, T2)> QueryFields<T1, T2>(string fieldName1, string fieldName2, string? whereClause = null, params MySqlParameter[] parameters)
    {
        string query = $"SELECT {fieldName1}, {fieldName2} FROM {m_TableName}";
        if (!string.IsNullOrEmpty(whereClause))
        {
            query += $" WHERE {whereClause}";
        }

        var connection = m_ConnectionProvider.GetConnection();
        try
        {
            using (var command = new MySqlCommand(query, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var value1 = reader.IsDBNull(0) ? default(T1) : (T1)Convert.ChangeType(reader.GetValue(0), typeof(T1));
                        var value2 = reader.IsDBNull(1) ? default(T2) : (T2)Convert.ChangeType(reader.GetValue(1), typeof(T2));
                        yield return (value1, value2);
                    }
                }
            }
        }
        finally
        {
            m_ConnectionProvider.RecycleConnection(connection);
        }
    }
}
