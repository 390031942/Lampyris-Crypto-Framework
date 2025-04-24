namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using MySql.Data.MySqlClient;
using System.Reflection;

[IniFile("db_connection.ini")]
public class DBConnectionConfig
{
    [IniField]
    public string ServerIP;

    [IniField]
    public string User;

    [IniField]
    public string Password;
}

public abstract class DBService:ILifecycle
{
    private MySqlConnection m_Connection;

    public abstract string DatebaseName { get; }

    public override int Priority => 0;

    public override void OnStart()
    {
        DBConnectionConfig dbConfig = IniConfigManager.Load<DBConnectionConfig>();
        if(dbConfig == null) 
        {
            Logger.LogError("Failed to connect database: db_connection.ini cannot be found");
            return;
        }

        string mySqlConnectStr = $"Server={dbConfig.ServerIP};" + 
                                 $"Database={DatebaseName};" +
                                 $"User={dbConfig.User};" +
                                 $"Password={dbConfig.Password};";

        m_Connection = new MySqlConnection(mySqlConnectStr);
        m_Connection.Open();
    }

    public override void OnDestroy()
    {
        if(m_Connection != null && m_Connection.State == System.Data.ConnectionState.Open)
        {
            m_Connection.Close();
        }
    }

    public DBTable<T> CreateTable<T>(params object[] tableNameArgs) where T : class, new()
    {
        var tableAttribute = typeof(T).GetCustomAttribute<DBTableAttribute>();
        if (tableAttribute == null)
        {
            throw new InvalidOperationException($"Class {typeof(T).Name} does not have a TableAttribute.");
        }

        var tableName = tableAttribute.GetTableName(tableNameArgs);
        var columns = new List<string>();

        foreach (var property in typeof(T).GetProperties())
        {
            var columnAttribute = property.GetCustomAttribute<DBColumnAttribute>();
            if (columnAttribute != null)
            {
                var columnDefinition = $"{columnAttribute.ColumnName} {columnAttribute.DataType}";
                if (columnAttribute.IsPrimaryKey)
                {
                    columnDefinition += " PRIMARY KEY";
                }
                if (columnAttribute.IsAutoIncrement)
                {
                    columnDefinition += " AUTO_INCREMENT";
                }
                if (columnAttribute.IsNotNull)
                {
                    columnDefinition += " NOT NULL";
                }
                columns.Add(columnDefinition);
            }
        }

        var createTableSql = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columns)});";

        using (var command = new MySqlCommand(createTableSql, m_Connection))
        {
            command.ExecuteNonQuery();
        }
        Console.WriteLine($"Table '{tableName}' created successfully.");


        // 返回 DBTable<T> 实例
        return new DBTable<T>(tableName, m_Connection);
    }

    public DBTable<T> GetTable<T>(params object[] tableNameArgs) where T : class, new()
    {
        // 获取表名
        var tableAttribute = typeof(T).GetCustomAttribute<DBTableAttribute>();
        if (tableAttribute == null)
        {
            throw new InvalidOperationException($"Class {typeof(T).Name} does not have a TableAttribute.");
        }

        var tableName = tableAttribute.GetTableName(tableNameArgs);

        // 检查表是否存在
        if (!TableExists(tableName))
        {
            Console.WriteLine($"Table '{tableName}' does not exist in the database.");
            return null;
        }

        // 返回 DBTable<T> 实例
        return new DBTable<T>(tableName, m_Connection);
    }

    private bool TableExists(string tableName)
    {
        const string query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @DatabaseName AND TABLE_NAME = @TableName";
        
        using (var command = new MySqlCommand(query, m_Connection))
        {
            // 获取当前数据库名称
            var databaseName = m_Connection.Database;

            // 添加参数化查询，防止 SQL 注入
            command.Parameters.AddWithValue("@DatabaseName", databaseName);
            command.Parameters.AddWithValue("@TableName", tableName);

            // 执行查询
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
    }

    public IEnumerable<T> QueryField<T>(string tableName, string fieldName, string whereClause = null, params MySqlParameter[] parameters)
    {
        // 构建查询 SQL
        string query = $"SELECT {fieldName} FROM {tableName}";
        if (!string.IsNullOrEmpty(whereClause))
        {
            query += $" WHERE {whereClause}";
        }

        using (var command = new MySqlCommand(query, m_Connection))
        {
            // 添加参数化查询，防止 SQL 注入
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // 每次迭代返回一个转换后的结果
                    yield return reader.IsDBNull(0) ? default : (T)Convert.ChangeType(reader.GetValue(0), typeof(T));
                }
            }
        }
    }

    public IEnumerable<(T1, T2)> QueryFields<T1, T2>(string tableName, string fieldName1, string fieldName2, string whereClause = null, params MySqlParameter[] parameters)
    {
        // 构建查询 SQL
        string query = $"SELECT {fieldName1}, {fieldName2} FROM {tableName}";
        if (!string.IsNullOrEmpty(whereClause))
        {
            query += $" WHERE {whereClause}";
        }

        using (var command = new MySqlCommand(query, m_Connection))
        {
            // 添加参数化查询，防止 SQL 注入
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // 每次迭代返回一个元组类型的结果
                    var value1 = reader.IsDBNull(0) ? default(T1) : (T1)Convert.ChangeType(reader.GetValue(0), typeof(T1));
                    var value2 = reader.IsDBNull(1) ? default(T2) : (T2)Convert.ChangeType(reader.GetValue(1), typeof(T2));
                    yield return (value1, value2);
                }
            }
        }
    }

}