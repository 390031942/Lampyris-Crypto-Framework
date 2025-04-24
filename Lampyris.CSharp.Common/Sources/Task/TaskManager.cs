using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lampyris.CSharp.Common;

public static class TaskManager
{
    private static readonly ConcurrentDictionary<int, TaskInfo> m_TaskBag = new ConcurrentDictionary<int, TaskInfo>();
    private static readonly PriorityQueue<TaskInfo, int> m_PriorityQueue = new PriorityQueue<TaskInfo, int>(); // 优先级队列
    private static int m_TaskIdCounter = 0;

    /// <summary>
    /// 创建并运行一个任务
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <param name="priority">任务优先级（数值越小优先级越高）</param>
    /// <param name="action">任务执行的操作</param>
    /// <returns>任务编号</returns>
    public static int RunTask(string taskName, int priority, Action<TaskProgress, CancellationToken> action)
    {
        int taskId = Interlocked.Increment(ref m_TaskIdCounter);

        string schedulerInfo = GetSchedulerInfo();

        var taskInfo = new TaskInfo
        {
            TaskId = taskId,
            TaskName = taskName,
            Priority = priority,
            Progress = new TaskProgress(),
            SchedulerInfo = schedulerInfo,
            StartTime = DateTime.Now,
            CancellationTokenSource = new CancellationTokenSource() // 创建任务的取消令牌
        };

        m_TaskBag[taskId] = taskInfo;

        // 将任务加入优先级队列
        m_PriorityQueue.Enqueue(taskInfo, priority);

        // 启动任务执行
        Task.Run(() =>
        {
            try
            {
                taskInfo.Status = TaskStatus.Running;
                action(taskInfo.Progress, taskInfo.CancellationTokenSource.Token);
                taskInfo.Status = TaskStatus.RanToCompletion;
            }
            catch (OperationCanceledException)
            {
                taskInfo.Status = TaskStatus.Canceled;
            }
            catch (Exception ex)
            {
                taskInfo.Status = TaskStatus.Faulted;
                taskInfo.Exception = ex;
            }
            finally
            {
                taskInfo.Progress.IsCompleted = true;
                taskInfo.EndTime = DateTime.Now;
                if (taskInfo.StartTime.HasValue && taskInfo.EndTime.HasValue)
                {
                    taskInfo.ElapsedMilliseconds = (taskInfo.EndTime.Value - taskInfo.StartTime.Value).TotalMilliseconds;
                }
            }
        });

        return taskId;
    }

    /// <summary>
    /// 获取任务信息
    /// </summary>
    /// <param name="taskId">任务编号</param>
    /// <returns>任务信息</returns>
    public static TaskInfo GetTaskInfo(int taskId)
    {
        m_TaskBag.TryGetValue(taskId, out var taskInfo);
        return taskInfo;
    }

    /// <summary>
    /// 获取所有任务信息
    /// </summary>
    /// <returns>所有任务信息</returns>
    public static TaskInfo[] GetAllTasks()
    {
        return m_TaskBag.Values.ToArray();
    }

    /// <summary>
    /// 获取指定状态的任务列表
    /// </summary>
    /// <param name="status">任务状态</param>
    /// <returns>筛选后的任务列表</returns>
    public static TaskInfo[] GetTasksByStatus(TaskStatus status)
    {
        return m_TaskBag.Values.Where(t => t.Status == status).ToArray();
    }

    /// <summary>
    /// 打印任务信息到控制台
    /// </summary>
    /// <param name="status">筛选任务状态（可选）</param>
    public static void PrintTasks(TaskStatus? status = null)
    {
        var tasks = status.HasValue ? GetTasksByStatus(status.Value) : GetAllTasks();

        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------");
        Console.WriteLine("| Task ID | Task Name           | Priority | Status           | Progress  | Message       | Scheduler Info             | Start Time           | End Time             | Elapsed (ms) |");
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------");

        foreach (var task in tasks)
        {
            Console.WriteLine($"| {task.TaskId,-7} | {task.TaskName,-18} | {task.Priority,-8} | {task.Status,-15} | {task.Progress.Percentage,8}% | {task.Progress.Message,-12} | {task.SchedulerInfo,-25} | {task.StartTime,-19} | {task.EndTime,-19} | {task.ElapsedMilliseconds,12:N0} |");
        }

        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------");
    }

    /// <summary>
    /// 取消指定任务
    /// </summary>
    /// <param name="taskId">任务编号</param>
    public static void CancelTask(int taskId)
    {
        if (m_TaskBag.TryGetValue(taskId, out var taskInfo))
        {
            taskInfo.CancellationTokenSource.Cancel();
            Console.WriteLine($"Task {taskId} has been canceled.");
        }
        else
        {
            Console.WriteLine($"Task {taskId} not found.");
        }
    }

    /// <summary>
    /// 获取优先级最高的任务
    /// </summary>
    /// <returns>优先级最高的任务</returns>
    public static TaskInfo GetHighestPriorityTask()
    {
        return m_PriorityQueue.TryPeek(out var task, out _) ? task : null;
    }

    private static string GetSchedulerInfo()
    {
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(2);
        var method = frame?.GetMethod();
        return method != null ? $"{method.DeclaringType?.FullName}.{method.Name}" : "Unknown";
    }
}