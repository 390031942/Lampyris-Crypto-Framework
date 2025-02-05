namespace Lampyris.CSharp.Common;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JobHandle
{
    // 跟踪所有任务的 Task
    private readonly List<Task> m_Tasks;

    public JobHandle(List<Task> tasks)
    {
        m_Tasks = tasks;
    }

    // 等待所有任务完成
    public void WaitForExecuteFinished()
    {
        Task.WaitAll(m_Tasks.ToArray());
    }

    // 检查是否所有任务都已完成
    public bool IsCompleted => m_Tasks.All(t => t.IsCompleted);
}
