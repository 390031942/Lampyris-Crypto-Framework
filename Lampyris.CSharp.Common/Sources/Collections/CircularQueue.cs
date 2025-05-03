namespace Lampyris.CSharp.Common;

using System.Collections.Generic;

public class CircularQueue<T>
{
    private readonly T[] m_buffer; // 缓冲区数组
    private int m_head;           // 队列头部索引
    private int m_count;          // 当前队列中的元素数量

    public CircularQueue(int capacity)
    {
        m_buffer = new T[capacity];
        m_head = 0;
        m_count = 0;
    }

    public int Count => m_count;

    /// <summary>
    /// 入队操作，将新元素添加到队列中。
    /// 如果队列已满，则覆盖最旧的元素。
    /// </summary>
    public void Enqueue(T item)
    {
        m_buffer[(m_head + m_count) % m_buffer.Length] = item;

        if (m_count < m_buffer.Length)
        {
            m_count++;
        }
        else
        {
            m_head = (m_head + 1) % m_buffer.Length; // 覆盖最旧的数据
        }
    }

    /// <summary>
    /// 通过索引访问队列中的元素。
    /// </summary>
    public T this[int index]
    {
        get => m_buffer[(m_head + index) % m_buffer.Length];
        set => m_buffer[(m_head + index) % m_buffer.Length] = value;
    }

    /// <summary>
    /// 将队列中的所有元素转换为 IEnumerable。
    /// </summary>
    public IEnumerable<T> ToEnumerable()
    {
        for (int i = 0; i < m_count; i++)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// 返回最后的 n 个元素构成的 ReadOnlySpan。
    /// 如果元素数量小于 n，则返回全部元素。
    /// </summary>
    /// <param name="n">需要返回的元素数量。</param>
    /// <returns>最后 n 个元素的 ReadOnlySpan。</returns>
    public ReadOnlySpan<T> AsLastSpan(int n)
    {
        // 如果元素数量小于 n，则返回全部元素
        n = Math.Min(n, m_count);

        // 计算最后 n 个元素的起始索引
        int startIndex = (m_head + m_count - n) % m_buffer.Length;

        // 如果数据是连续的（没有环绕）
        if (startIndex + n <= m_buffer.Length)
        {
            return new ReadOnlySpan<T>(m_buffer, startIndex, n);
        }

        // 如果数据是环绕的（跨越了数组的末尾）
        T[] tempBuffer = new T[n];
        int firstPartLength = m_buffer.Length - startIndex;
        Array.Copy(m_buffer, startIndex, tempBuffer, 0, firstPartLength);
        Array.Copy(m_buffer, 0, tempBuffer, firstPartLength, n - firstPartLength);

        return new ReadOnlySpan<T>(tempBuffer);
    }

    /// <summary>
    /// 清空队列中的所有元素。
    /// </summary>
    public void Clear()
    {
        // 重置头部索引和计数器
        m_head = 0;
        m_count = 0;

        // 可选：清空缓冲区中的数据（仅在需要时）
        Array.Clear(m_buffer, 0, m_buffer.Length);
    }

    public T First()
    {
        return Count > 0 ? this[0] : default;
    }

    public T Last()
    {
        return Count > 0 ? this[Count - 1] : default;
    }
}