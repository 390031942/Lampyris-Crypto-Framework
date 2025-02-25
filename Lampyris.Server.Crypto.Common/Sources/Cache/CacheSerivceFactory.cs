namespace Lampyris.Server.Crypto.Common;

using System;
using System.Collections.Concurrent;

public enum CacheServiceType
{
    MemoryCache,
    RedisCache
}

public static class CacheServiceFactory
{
    private static readonly ConcurrentDictionary<CacheServiceType, ICacheService> m_CacheServices = new();

    public static ICacheService Get(CacheServiceType cacheServiceType)
    {
        // 如果实例已存在，则直接返回；否则创建新实例并存储
        return m_CacheServices.GetOrAdd(cacheServiceType, type =>
        {
            return type switch
            {
                CacheServiceType.MemoryCache => new MemoryCacheService(),
                CacheServiceType.RedisCache => new RedisCacheService(),
                _ => throw new ArgumentException($"Unsupported cache service type: {cacheServiceType}")
            };
        });
    }
}
