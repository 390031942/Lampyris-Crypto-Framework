namespace Lampyris.CSharp.Common;

using System.Collections.Concurrent;
using System.Reflection;

public static class ReflectionCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    public static PropertyInfo[] GetProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, t => t.GetProperties());
    }
}