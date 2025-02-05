namespace Lampyris.CSharp.Common;

using System;
using System.Collections.Generic;

public abstract class Job
{
    private readonly List<Job> m_Dependencies = new List<Job>();
    public string              Name { get; }
    public bool                IsCompleted { get; private set; } = false;

    public Job(string name)
    {
        Name = name;
    }

    // 添加依赖任务
    public void AddDependency(Job job)
    {
        if (job != null && !m_Dependencies.Contains(job))
        {
            m_Dependencies.Add(job);
        }
    }

    // 获取所有依赖任务
    public IReadOnlyList<Job> GetDependencies()
    {
        return m_Dependencies.AsReadOnly();
    }

    // 检查是否有未完成的依赖任务
    public bool HasUncompletedDependencies()
    {
        foreach (var dependency in m_Dependencies)
        {
            if (!dependency.IsCompleted)
                return true;
        }
        return false;
    }

    // 执行任务
    public void Execute()
    {
        if (HasUncompletedDependencies())
            throw new InvalidOperationException($"任务 {Name} 的前置任务尚未完成！");

        Run(); // 执行任务逻辑
        IsCompleted = true;
    }

    // 任务逻辑，由子类实现
    protected abstract void Run();
}
