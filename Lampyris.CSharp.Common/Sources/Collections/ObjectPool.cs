using System.Collections.Concurrent;

namespace Lampyris.CSharp.Common;

public class ObjectPool<T> where T : new()
{
    private readonly ConcurrentBag<T> m_Pool = new ConcurrentBag<T>();

    public T Get()
    {
        if (m_Pool.TryTake(out T item))
        {
            return item;
        }
        return new T();
    }

    public void Recycle(T item)
    {
        m_Pool.Add(item);
    }
}

public class ObjectListPool<T>
{
    private readonly ConcurrentBag<List<T>> m_Pool = new ConcurrentBag<List<T>>();

    public List<T> Get()
    {
        if (m_Pool.TryTake(out var list))
        {
            return list;
        }
        return new List<T>();
    }

    public void Recycle(List<T> list)
    {
        list.Clear();
        m_Pool.Add(list);
    }
}