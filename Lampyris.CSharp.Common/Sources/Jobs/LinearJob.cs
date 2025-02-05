namespace Lampyris.CSharp.Common;

public class LinearJob : Job
{
    private readonly Action m_Action;

    public LinearJob(string name, Action action) : base(name)
    {
        m_Action = action;
    }

    protected override void Run()
    {
        m_Action();
    }
}
