namespace Lampyris.CSharp.Common;

using System.Collections;

public abstract class AsyncOperation:IEnumerator
{
    protected float m_Progress;

    protected bool m_Finished;

    public virtual float Progress => m_Progress;

    public virtual bool Finished => m_Finished;

    public virtual object? Current => null;

    public virtual bool MoveNext()
    {
        return false;
    }

    public virtual void Reset()
    {
        throw new NotImplementedException();
    }
}
