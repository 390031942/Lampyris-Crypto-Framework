namespace Lampyris.CSharp.Common;

public class TaskProgress
{
    public int Percentage { get; set; } = 0;
    public string Message { get; set; } = "";
    public bool IsCompleted { get; set; } = false;
}