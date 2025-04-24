namespace Lampyris.CSharp.Common;

public class TaskInfo
{
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public int Priority { get; set; } // 任务优先级
    public TaskStatus Status { get; set; } = TaskStatus.Created;
    public TaskProgress Progress { get; set; }
    public Exception Exception { get; set; }
    public string SchedulerInfo { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? ElapsedMilliseconds { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
}
