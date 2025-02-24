namespace Lampyris.Server.Crypto.Common;

using StackExchange.Redis;
using System.Text.Json;

public class RedisCacheService
{
    private readonly ConnectionMultiplexer m_Redis;
    private readonly IDatabase m_DB;

    public RedisCacheService(string connectionString)
    {
        m_Redis = ConnectionMultiplexer.Connect(connectionString);
        m_DB = m_Redis.GetDatabase();
    }

    public void Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        m_DB.StringSet(key, json, expiry);
    }

    public T? Get<T>(string key)
    {
        var json = m_DB.StringGet(key);
        if (json.IsNullOrEmpty)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(json);
    }

    public bool Exists(string key)
    {
        return m_DB.KeyExists(key);
    }
}
