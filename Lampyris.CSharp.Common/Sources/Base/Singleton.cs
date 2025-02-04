namespace Lampyris.CSharp.Common;

public class Singleton<T> where T : class, new()
{
    private static T? m_Instance;

    public static T Instance
    {
        get { return m_Instance ??= new T(); }
    }
}
