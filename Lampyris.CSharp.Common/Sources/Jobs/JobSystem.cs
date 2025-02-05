namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JobSystem
{
    private readonly List<Job> m_Jobs = new List<Job>();

    // 添加任务
    public void AddJob(Job job)
    {
        m_Jobs.Add(job);
    }

    public JobHandle ExecuteAll()
    {
        // 拓扑排序
        var sortedJobs = TopologicalSort(m_Jobs);

        // 创建任务列表
        var tasks = new List<Task>();

        // 按顺序调度任务
        foreach (var job in sortedJobs)
        {
            var task = Task.Run(() =>
            {
                job.Execute();
            });
            tasks.Add(task);
        }

        return new JobHandle(tasks);
    }

    // 拓扑排序
    private List<Job> TopologicalSort(List<Job> jobs)
    {
        var sorted = new List<Job>();
        var inDegree = new Dictionary<Job, int>(); // 记录每个任务的入度

        // 初始化入度
        foreach (var job in jobs)
        {
            if (!inDegree.ContainsKey(job))
                inDegree[job] = 0;

            foreach (var dependency in job.GetDependencies())
            {
                if (!inDegree.ContainsKey(dependency))
                    inDegree[dependency] = 0;

                inDegree[job]++;
            }
        }

        // 找到所有入度为 0 的任务
        var queue = new Queue<Job>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            sorted.Add(current);

            // 移除当前任务的依赖关系
            foreach (var job in jobs)
            {
                if (job.GetDependencies().Contains(current))
                {
                    inDegree[job]--;
                    if (inDegree[job] == 0)
                        queue.Enqueue(job);
                }
            }
        }

        // 检查是否存在循环依赖
        if (sorted.Count != jobs.Count)
        {
            sorted.Clear();
        }

        return sorted;
    }
}
