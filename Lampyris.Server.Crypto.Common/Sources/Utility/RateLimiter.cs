namespace Lampyris.Server.Crypto.Common;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class RateLimiter
{
    // 最大调用次数
    private readonly int             m_MaxCalls;

    // 时间窗口
    private readonly TimeSpan        m_TimeWindow;

    // 调用时间记录
    private readonly Queue<DateTime> m_CallTimestamps;

    // 控制并发访问
    private readonly SemaphoreSlim   m_Semaphore; 

    public RateLimiter(int maxCalls, TimeSpan timeWindow)
    {
        if (maxCalls <= 0)
            throw new ArgumentException("Max calls must be greater than 0.", nameof(maxCalls));
        if (timeWindow.TotalMilliseconds <= 0)
            throw new ArgumentException("Time window must be greater than 0.", nameof(timeWindow));

        m_MaxCalls       = maxCalls;
        m_TimeWindow     = timeWindow;
        m_CallTimestamps = new Queue<DateTime>();
        m_Semaphore      = new SemaphoreSlim(1, 1); // 用于线程安全
    }

    /// <summary>
    /// 尝试调用，如果超过限制则等待。
    /// </summary>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await m_Semaphore.WaitAsync(cancellationToken); // 确保线程安全
        try
        {
            DateTime now = DateTime.UtcNow;

            // 移除超出时间窗口的调用记录
            while (m_CallTimestamps.Count > 0 && (now - m_CallTimestamps.Peek()) > m_TimeWindow)
            {
                m_CallTimestamps.Dequeue();
            }

            // 如果调用次数已达到限制，则计算需要等待的时间
            if (m_CallTimestamps.Count >= m_MaxCalls)
            {
                DateTime oldestCall = m_CallTimestamps.Peek();
                TimeSpan waitTime = m_TimeWindow - (now - oldestCall);

                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime, cancellationToken); // 等待
                }
            }

            // 记录当前调用时间
            m_CallTimestamps.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            m_Semaphore.Release(); // 释放信号量
        }
    }
}
