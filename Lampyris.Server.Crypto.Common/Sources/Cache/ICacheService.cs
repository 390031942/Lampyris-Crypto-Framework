namespace Lampyris.Server.Crypto.Common;
public interface ICacheService
{
    // 设置缓存
    void Set<T>(string key, T value, TimeSpan? expiry = null);

    // 获取缓存
    T? Get<T>(string key);

    // 检查键是否存在
    bool ContainsKey(string key);

    // 删除缓存
    void Remove(string key);
}
