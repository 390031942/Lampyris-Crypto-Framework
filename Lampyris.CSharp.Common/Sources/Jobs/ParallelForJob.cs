namespace Lampyris.CSharp.Common;

using System.Threading.Tasks;

public class ParallelForJob<T> : Job
{
    private readonly T[] m_Data; // 循环任务的数据
    private readonly Action<T, int> m_Action; // 每个数据项的处理逻辑
    private readonly int m_ChunkSize; // 分割大小

    public ParallelForJob(string name, T[] data, Action<T, int> action, int chunkSize = 10) : base(name)
    {
        m_Data = data;
        m_Action = action;
        m_ChunkSize = chunkSize;
    }

    protected override void Run()
    {
        // 分割任务并行执行
        Parallel.For(0, m_Data.Length / m_ChunkSize + 1, chunkIndex =>
        {
            int start = chunkIndex * m_ChunkSize;
            int end = Math.Min(start + m_ChunkSize, m_Data.Length);

            for (int i = start; i < end; i++)
            {
                m_Action(m_Data[i], i); // 执行每个数据项的逻辑
            }
        });
    }
}
