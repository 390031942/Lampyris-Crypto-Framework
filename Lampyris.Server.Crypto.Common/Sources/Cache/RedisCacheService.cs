namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using StackExchange.Redis;
using System.Text.Json;

[IniFile("redis_connection.ini")]
public class RedisConnectionConfig
{
    [IniField]
    public string ServerIP;

    [IniField]
    public int    Port;
}

[Component]
public class RedisCacheService : ICacheService
{
    private readonly ConnectionMultiplexer m_Redis;
    private readonly IDatabase             m_DB;

    public RedisCacheService()
    {
        // 加载 Redis 配置
        RedisConnectionConfig config = IniConfigManager.Load<RedisConnectionConfig>();
        m_Redis = ConnectionMultiplexer.Connect($"{config.ServerIP}:{config.Port}");
        m_DB = m_Redis.GetDatabase();
    }

    // 设置缓存
    public void Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        m_DB.StringSet(key, json, expiry);
    }

    // 获取缓存
    public T? Get<T>(string key)
    {
        var json = m_DB.StringGet(key);
        if (json.IsNullOrEmpty)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(json);
    }

    // 检查键是否存在
    public bool ContainsKey(string key)
    {
        return m_DB.KeyExists(key);
    }

    // 删除缓存
    public void Remove(string key)
    {
        m_DB.KeyDelete(key);
    }
}
