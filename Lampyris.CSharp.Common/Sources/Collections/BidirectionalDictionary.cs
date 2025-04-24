namespace Lampyris.CSharp.Common;

using System.Collections;

public class BidirectionalDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    private readonly Dictionary<TKey, TValue> _forward = new();
    private readonly Dictionary<TValue, TKey> _reverse = new();

    public void Add(TKey key, TValue value)
    {
        if (_forward.ContainsKey(key))
            throw new ArgumentException($"Key '{key}' already exists in the dictionary.");
        if (_reverse.ContainsKey(value))
            throw new ArgumentException($"Value '{value}' already exists in the dictionary.");

        _forward[key] = value;
        _reverse[value] = key;
    }

    public bool TryGetByKey(TKey key, out TValue value)
    {
        return _forward.TryGetValue(key, out value);
    }

    public bool TryGetByValue(TValue value, out TKey key)
    {
        return _reverse.TryGetValue(value, out key);
    }

    // 新增 TryGetValue 方法（针对 Key）
    public bool TryGetValue(TKey key, out TValue value)
    {
        return TryGetByKey(key, out value);
    }

    // 新增 TryGetValue 方法（针对 Value）
    public bool TryGetValue(TValue value, out TKey key)
    {
        return TryGetByValue(value, out key);
    }

    public TValue GetByKey(TKey key)
    {
        if (!_forward.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");
        return value;
    }

    public TKey GetByValue(TValue value)
    {
        if (!_reverse.TryGetValue(value, out var key))
            throw new KeyNotFoundException($"Value '{value}' not found in the dictionary.");
        return key;
    }

    public void RemoveByKey(TKey key)
    {
        if (!_forward.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");

        _forward.Remove(key);
        _reverse.Remove(value);
    }

    public void RemoveByValue(TValue value)
    {
        if (!_reverse.TryGetValue(value, out var key))
            throw new KeyNotFoundException($"Value '{value}' not found in the dictionary.");

        _reverse.Remove(value);
        _forward.Remove(key);
    }

    public int Count => _forward.Count;

    public IEnumerable<TKey> Keys => _forward.Keys;

    public IEnumerable<TValue> Values => _reverse.Keys;

    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }

    // 索引器：通过键访问值
    public TValue this[TKey key]
    {
        get => GetByKey(key);
        set
        {
            if (_forward.ContainsKey(key))
            {
                // 如果键已存在，更新值，同时更新反向映射
                var oldValue = _forward[key];
                _reverse.Remove(oldValue);
            }
            if (_reverse.ContainsKey(value))
            {
                // 如果值已存在，更新键，同时更新正向映射
                var oldKey = _reverse[value];
                _forward.Remove(oldKey);
            }

            _forward[key] = value;
            _reverse[value] = key;
        }
    }

    // 实现 IEnumerable<KeyValuePair<TKey, TValue>>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _forward.GetEnumerator();
    }

    // 实现 IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}