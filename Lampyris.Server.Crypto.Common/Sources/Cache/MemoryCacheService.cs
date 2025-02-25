namespace Lampyris.Server.Crypto.Common;

using System.Runtime.Caching;

public class MemoryCacheService: ICacheService
{
    private readonly MemoryCache m_Cache;

    public MemoryCacheService()
    {
        m_Cache = MemoryCache.Default;
    }

    // 设置缓存
    public void Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        var cacheItemPolicy = new CacheItemPolicy();
        if (expiry.HasValue)
        {
            cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.Add(expiry.Value);
        }
        m_Cache.Set(key, value, cacheItemPolicy);
    }

    // 获取缓存
    public T? Get<T>(string key)
    {
        if (m_Cache.Contains(key))
        {
            return (T)m_Cache.Get(key);
        }
        return default;
    }

    // 检查键是否存在
    public bool Exists(string key)
    {
        return m_Cache.Contains(key);
    }

    // 删除缓存
    public void Remove(string key)
    {
        if (m_Cache.Contains(key))
        {
            m_Cache.Remove(key);
        }
    }
}