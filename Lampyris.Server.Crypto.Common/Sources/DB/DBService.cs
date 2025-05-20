namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using MySql.Data.MySqlClient;
    using System.Collections.Concurrent;
using System.Data.Common;
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

public interface IDBConnectionProvider
{
    public MySqlConnection GetConnection();
    public void RecycleConnection(MySqlConnection connection);
}

public abstract class DBService : ILifecycle, IDBConnectionProvider
{
    private readonly ConcurrentBag<MySqlConnection> m_ConnectionPool = new ConcurrentBag<MySqlConnection>();
    private int m_MaxConnections;

    public abstract string DatebaseName { get; }

    public override int Priority => 0;

    public override void OnStart()
    {
        DBConnectionConfig dbConfig = IniConfigManager.Load<DBConnectionConfig>();
        if (dbConfig == null)
        {
            Logger.LogError("Failed to connect database: db_connection.ini cannot be found");
            return;
        }

        string mySqlConnectStr = $"Server={dbConfig.ServerIP};" +
                                 $"Database={DatebaseName};" +
                                 $"User={dbConfig.User};" +
                                 $"Password={dbConfig.Password};" +
                                 $"ConnectionLifeTime = 999999999;";

        // 获取 Task 的最大并发数
        m_MaxConnections = Environment.ProcessorCount + 1;

        // 初始化连接池
        for (int i = 0; i < m_MaxConnections; i++)
        {
            var connection = new MySqlConnection(mySqlConnectStr);
            connection.Open();
            m_ConnectionPool.Add(connection);
        }
    }

    public override void OnDestroy()
    {
        while (m_ConnectionPool.TryTake(out var connection))
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
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
        return new DBTable<T>(tableName, this);
    }

    public DBTable<T> GetTable<T>(string tableName) where T : class, new()
    {
        // 检查表是否存在
        if (!TableExists(tableName))
        {
            Console.WriteLine($"Table '{tableName}' does not exist in the database.");
            return null;
        }

        // 返回 DBTable<T> 实例
        return new DBTable<T>(tableName, this);
    }
    public DBTable<T> CreateTable<T>(string tableName) where T : class, new()
    {
        var columns = new List<string>();

        foreach (var member in typeof(T).GetMembers())
        {
            if (member is PropertyInfo || member is FieldInfo)
            {
                var columnAttribute = member.GetCustomAttribute<DBColumnAttribute>();
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
        }

        var createTableSql = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columns)});";

        var connection = GetConnection();
        try
        {
            using (var command = new MySqlCommand(createTableSql, connection))
            {
                command.ExecuteNonQuery();
            }
            Console.WriteLine($"Table '{tableName}' created successfully.");
        }
        finally
        {
            RecycleConnection(connection);
        }

        // 返回 DBTable<T> 实例
        return new DBTable<T>(tableName, this);
    }

    public DBTable<T> CreateTable<T>(params object[] tableNameArgs) where T : class, new()
    {
        var tableAttribute = typeof(T).GetCustomAttribute<DBTableAttribute>();
        if (tableAttribute == null)
        {
            throw new InvalidOperationException($"Class {typeof(T).Name} does not have a TableAttribute.");
        }

        var tableName = tableAttribute.GetTableName(tableNameArgs);
        return CreateTable<T>(tableName);
    }

    public bool TableExists(string tableName)
    {
        const string query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @DatabaseName AND TABLE_NAME = @TableName";

        var connection = GetConnection();
        try
        {
            using (var command = new MySqlCommand(query, connection))
            {
                // 获取当前数据库名称
                var databaseName = connection.Database;

                // 添加参数化查询，防止 SQL 注入
                command.Parameters.AddWithValue("@DatabaseName", databaseName);
                command.Parameters.AddWithValue("@TableName", tableName);

                // 执行查询
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
        }
        finally
        {
            RecycleConnection(connection);
        }
    }

    public MySqlConnection GetConnection()
    {
        if (m_ConnectionPool.TryTake(out var connection))
        {
            return connection;
        }
        throw new InvalidOperationException("No available connections in the pool.");
    }

    public void RecycleConnection(MySqlConnection connection)
    {
        if (connection != null)
        {
            m_ConnectionPool.Add(connection);
        }
    }
}