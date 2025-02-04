namespace Lampyris.CSharp.Common;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public interface IPostSerializationHandler
{
    public void PostSerialization();
}

/*
 * 可序列化的单例类，比如App设置等
 */
[Serializable]
public class SerializableSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> where T : class, new()
{
    private static T? ms_instance;

    public static T Instance
    {
        get
        {
            if (ms_instance == null)
            {
                ms_instance = SerializationManager.Instance.TryDeserializeObjectFromFile<T>();
                if(ms_instance == null)
                {
                    ms_instance = new T();
                }
                else
                {
                    // 检查类型T是否实现了IPostSerializationHandler接口
                    if (typeof(IPostSerializationHandler).IsAssignableFrom(typeof(T)))
                    {
                        MethodInfo? method = typeof(T).GetMethod("PostSerialization");
                        if (method != null)
                        {
                            method.Invoke(ms_instance, null); // 通过反射调用func方法
                        }
                    }
                }

                SerializationManager.Instance.Register(ms_instance);
            }
            return ms_instance;
        }
    }

    public virtual void PostSerialization() { }
}