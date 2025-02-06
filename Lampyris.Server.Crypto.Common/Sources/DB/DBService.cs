namespace Lampyris.CSharp.Common;

using MySql.Data.MySqlClient;

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
}