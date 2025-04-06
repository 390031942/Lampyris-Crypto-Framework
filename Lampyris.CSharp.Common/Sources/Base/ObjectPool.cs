using System.Collections.Concurrent;

namespace Lampyris.CSharp.Common;

public class ObjectPool<T> where T : new()
{
    private readonly ConcurrentBag<T> m_pool = new ConcurrentBag<T>();

    public T Get()
    {
        if (m_pool.TryTake(out T item))
        {
            return item;
        }
        return new T();
    }

    public void Recycle(T item)
    {
        m_pool.Add(item);
    }
}
