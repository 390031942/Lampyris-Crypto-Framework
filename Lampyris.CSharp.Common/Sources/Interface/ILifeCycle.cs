namespace Lampyris.CSharp.Common;

public class ILifecycle
{
    public virtual void OnStart() { }

    public virtual void OnUpdate() { }

    public virtual void OnDestroy() { }
}

