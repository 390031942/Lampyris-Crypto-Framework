using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class DatabaseService
{
    private const string ConnectionString = "Server=localhost;Port=3306;Database=lampyris.server.crypto.binance;Uid=root;Pwd=lampyris-dev;SslMode=Required;";

    public DatabaseService()
    {
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
        }
    }

    public void CreateTableIfNotExists(string symbol, string interval)
    {
        string tableName = $"{symbol}_{interval}";
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            string createTableQuery = $@"
                CREATE TABLE IF NOT EXISTS `{tableName}` (
                    `datetime` DATETIME NOT NULL PRIMARY KEY,
                    `open` DOUBLE NOT NULL,
                    `close` DOUBLE NOT NULL,
                    `low` DOUBLE NOT NULL,
                    `high` DOUBLE NOT NULL,
                    `volume` DOUBLE NOT NULL,
                    `currency` DOUBLE NOT NULL
                );
            ";
            using (var command = new MySqlCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public void SaveKlineData(string symbol, string interval, List<List<object>> data)
    {
        string tableName = $"{symbol}_{interval}";
        using (var connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var row in data)
                {
                    string insertQuery = $@"
                        INSERT IGNORE INTO `{tableName}` 
                        (`datetime`, `open`, `close`, `low`, `high`, `volume`, `currency`)
                        VALUES (@DateTime, @Open, @Close, @Low, @High, @Volume, @Currency);
                    ";
                    using (var command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DateTime", DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(row[0])).UtcDateTime);
                        command.Parameters.AddWithValue("@Open", row[1]);
                        command.Parameters.AddWithValue("@Close", row[4]);
                        command.Parameters.AddWithValue("@Low", row[3]);
                        command.Parameters.AddWithValue("@High", row[2]);
                        command.Parameters.AddWithValue("@Volume", row[5]);
                        command.Parameters.AddWithValue("@Currency", row[7]);

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
    }
}
