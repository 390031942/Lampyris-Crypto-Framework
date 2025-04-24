namespace Lampyris.CSharp.Common;

public static class UniqueIdGenerator
{
    private static readonly object ms_Lock = new object();
    private static long ms_LastTimestamp = 0;
    private static int m_Sequence = 0;
    private const int m_MaxSequence = 999; // 每毫秒最多支持生成1000个唯一ID

    /// <summary>
    /// 生成唯一订单ID
    /// </summary>
    /// <returns>long类型的唯一订单ID</returns>
    public static long Get()
    {
        lock (ms_Lock)
        {
            long timestamp = GetCurrentTimestamp();

            if (timestamp == ms_LastTimestamp)
            {
                // 如果在同一毫秒内，增加序列号
                m_Sequence = (m_Sequence + 1) % m_MaxSequence;

                // 如果序列号溢出，等待下一毫秒
                if (m_Sequence == 0)
                {
                    timestamp = WaitForNextTimestamp(ms_LastTimestamp);
                }
            }
            else
            {
                // 如果是新的一毫秒，重置序列号
                m_Sequence = 0;
            }

            ms_LastTimestamp = timestamp;

            // 订单ID由时间戳和序列号组成
            return timestamp * 1000 + m_Sequence;
        }
    }

    /// <summary>
    /// 获取当前时间戳（毫秒级）
    /// </summary>
    /// <returns>当前时间戳</returns>
    private static long GetCurrentTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// 等待下一毫秒
    /// </summary>
    /// <param name="lastTimestamp">上一次的时间戳</param>
    /// <returns>新的时间戳</returns>
    private static long WaitForNextTimestamp(long lastTimestamp)
    {
        long timestamp;
        do
        {
            timestamp = GetCurrentTimestamp();
        } while (timestamp <= lastTimestamp);

        return timestamp;
    }
}
