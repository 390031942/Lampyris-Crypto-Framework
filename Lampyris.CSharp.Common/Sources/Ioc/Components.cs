namespace Lampyris.CSharp.Common;

using System.Collections.ObjectModel;
using System.Reflection;

public static class Components
{
    /// <summary>
    /// 存储组件实例
    /// </summary>
    private static readonly Dictionary<Type, object> m_Components = new();

    /// <summary>
    /// 存储按名称注册的组件
    /// 其中Type是抽象一个类
    /// </summary>
    private static Dictionary<Type, Dictionary<string,object>> m_NamedComponents = new();

    /// <summary>
    /// 存储按名称注册的组件
    /// 其中Type是抽象一个类
    /// </summary>
    private static Dictionary<string, List<object>> m_Tag2Components = new();

    /// <summary>
    /// 存储实现了 ILifecycle 的组件
    /// </summary>
    private static readonly List<ILifecycle> m_LifecycleComponents = new();

    /// <summary>
    /// 是否已经注册
    /// </summary>
    private static bool m_IsRegistered = false;

    // 扫描并注册所有标记为 [Component] 的类
    public static void RegisterComponents(List<Assembly> assemblies)
    {
        if (assemblies == null || assemblies.Count == 0 || m_IsRegistered)
            return;

        // 获取所有标记了 [Component] 的类
        List<Type> componentTypes = new List<Type>();
        foreach (Assembly assembly in assemblies)
        {
            componentTypes.AddRange(assembly.GetTypes()
            .Where(t => t.IsClass && t.GetCustomAttribute<ComponentAttribute>() != null));
        }
        
        componentTypes.Sort((lhs,rhs) => 
        {
            if (lhs.IsAbstract && !rhs.IsAbstract) return -1; // 抽象类排在前面
            if (!lhs.IsAbstract && rhs.IsAbstract) return 1;  // 非抽象类排在后面
            return 0; // 两者相等
        });

        foreach (var type in componentTypes)
        {
            if (type.IsAbstract)
            {
                m_NamedComponents[type] = new Dictionary<string, object>();
            }
            else
            {
                // 创建实例并注册到容器中
                var instance = Activator.CreateInstance(type);
                if (instance != null)
                {
                    m_Components[type] = instance;

                    ComponentAttribute? attribute = type.GetCustomAttribute<ComponentAttribute>();
                    if (attribute != null)
                    {
                        if (type.BaseType != null && type.BaseType.IsAbstract)
                        {
                            Type? baseType = type.BaseType;
                            while (baseType != null)
                            {
                                if (baseType.IsAbstract && !baseType.IsGenericType) // 自顶向下遍历所有继承的非泛型的抽象类
                                {
                                    if (!string.IsNullOrEmpty(attribute.Name))
                                    {
                                        if (m_NamedComponents.ContainsKey(type))
                                        {
                                            var dict = m_NamedComponents[type];
                                            if (dict.ContainsKey(attribute.Name)) // 确保组件名称的唯一性
                                            {
                                                throw new InvalidOperationException($"Duplicated component Name \"{attribute.Name}\"");
                                            }
                                            m_NamedComponents[type][attribute.Name] = instance;
                                        }
                                    }
                                    else
                                    {
                                        if(!m_Components.ContainsKey(baseType))
                                        {
                                            m_Components[baseType] = instance;
                                        }
                                        else
                                        {
                                            throw new InvalidOperationException($"Duplicated name-less component instance, base type = {baseType.Name}");
                                        }
                                    }
                                }

                                baseType = baseType.BaseType;
                            }
                        }
       
                        if (!string.IsNullOrEmpty(attribute.Tag))
                        {
                            if (!m_Tag2Components.ContainsKey(attribute.Tag))
                            {
                                m_Tag2Components[attribute.Tag] = new List<object>();
                            }

                            m_Tag2Components[attribute.Tag].Add(instance);
                        }
                    }

                    // 如果组件实现了 ILifecycle，则加入生命周期管理列表
                    if (instance is ILifecycle lifecycleComponent)
                    {
                        m_LifecycleComponents.Add(lifecycleComponent);
                    }
                }
            }
        }

        // 按优先级排序生命周期组件
        m_LifecycleComponents.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        m_IsRegistered = true;
    }

    // 自动注入 [Autowired] 标记的字段和属性
    // 规则: 
    // 1) 如果Type是一个非抽象类，则直接返回对应示例
    // 2) 如果Type是一个抽象类，则找到对应名称的组件示例
    public static void PerformDependencyInjection()
    {
        foreach (var component in m_Components.Values)
        {
            var componentType = component.GetType();

            // 注入字段
            var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(f => f.GetCustomAttribute<AutowiredAttribute>() != null);

            foreach (var field in fields)
            {
                AutowiredAttribute? attribute = field.GetCustomAttribute<AutowiredAttribute>();
                var dependency = ResolveDependency(field.FieldType, attribute != null ? attribute.Name : null);
                field.SetValue(component, dependency);
            }

            // 注入属性
            var properties = componentType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(p => p.GetCustomAttribute<AutowiredAttribute>() != null);

            foreach (var property in properties)
            {
                if(property.CanWrite)
                {
                    AutowiredAttribute? attribute = property.GetCustomAttribute<AutowiredAttribute>();
                    var dependency = ResolveDependency(property.PropertyType, attribute != null ? attribute.Name : null);
                    property.SetValue(component, dependency);
                }
            }
        }
    }

    // 解析依赖
    private static object ResolveDependency(Type type, string? name)
    {
        if(!type.IsAbstract || string.IsNullOrEmpty(name))
        {
            if (m_Components.TryGetValue(type, out var dependency))
            {
                return dependency;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(name))
            {
                if(m_NamedComponents.TryGetValue(type, out var dict))
                {
                    if (dict.ContainsKey(name))
                    {
                        return dict[name];
                    }
                }
            }
        }
        throw new InvalidOperationException($"No component found for type {type.FullName}");
    }

    // 获取组件实例
    public static T GetComponent<T>()
    {
        return (T)m_Components[typeof(T)];
    }

    public static ReadOnlyCollection<object> GetComponentsByTag(string tag)
    {
        m_Tag2Components.TryGetValue(tag, out var componentList);
        return componentList?.AsReadOnly();
    }

    /// <summary>
    /// 调用所有生命周期组件的 OnStart 方法
    /// </summary>
    public static void StartLifecycle()
    {
        foreach (var lifecycleComponent in m_LifecycleComponents)
        {
            lifecycleComponent.OnStart();
        }
    }

    /// <summary>
    /// 调用所有生命周期组件的 OnUpdate 方法
    /// </summary>
    public static void UpdateLifecycle()
    {
        foreach (var lifecycleComponent in m_LifecycleComponents)
        {
            lifecycleComponent.OnUpdate();
        }
    }

    /// <summary>
    /// 调用所有生命周期组件的 OnDestroy 方法
    /// </summary>
    public static void DestroyLifecycle()
    {
        foreach (var lifecycleComponent in m_LifecycleComponents)
        {
            lifecycleComponent.OnDestroy();
        }
    }
}