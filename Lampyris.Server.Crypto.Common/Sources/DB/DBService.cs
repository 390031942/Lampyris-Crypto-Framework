namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using MySql.Data.MySqlClient;
using System.Reflection;

[IniFile(FileName = "db_connection.ini")]
public class DBConnectionConfig
{
    public string serverIP;
    public string databaseName;
    public string user;
    public string password;
}

[Component]
public class DBService:ILifecycle
{
    private MySqlConnection m_Connection;

    public override void OnStart()
    {
        DBConnectionConfig dbConfig = IniConfigManager.GetConfig<DBConnectionConfig>();
        if(dbConfig == null) 
        {
            Logger.LogError("Failed to connect database: db_connection.ini cannot be found");
            Application.Quit();
            return;
        }

        string mySqlConnectStr = $"Server={dbConfig.serverIP};" + 
                                  "Database={dbConfig.databaseName};" +
                                  "User={dbConfig.user};" + 
                                  "Password={dbConfig.databaseName};";

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

    public void CreateTable<T>(params object[] tableNameArgs)
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
                columns.Add(columnDefinition);
            }
        }

        var createTableSql = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columns)});";

        using (var command = new MySqlCommand(createTableSql, m_Connection))
        {
            command.ExecuteNonQuery();
        }
        Console.WriteLine($"Table '{tableName}' created successfully.");
    }

    public DBTable<T> GetTable<T>(params object[] tableNameArgs) where T : new()
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
}