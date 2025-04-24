namespace Lampyris.CSharp.Common;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Linq;

public static class Components
{
    /// <summary>
    /// 存储组件实例
    /// </summary>
    private static readonly Dictionary<Type, object> m_Components = new();

    /// <summary>
    /// 存储按名称注册的组件
    /// </summary>
    private static readonly Dictionary<string, object> m_NamedComponents = new();

    /// <summary>
    /// 存储tag->组件列表
    /// </summary>
    private static readonly Dictionary<string, List<object>> m_Tag2Components = new();

    /// <summary>
    /// 存储实现了 ILifecycle 的组件
    /// </summary>
    private static readonly List<ILifecycle> m_LifecycleComponents = new();

    // 扫描并注册所有标记为 [Component] 的类
    public static void RegisterComponents(Assembly? assembly)
    {
        if (assembly == null)
            return;

        // 获取所有标记了 [Component] 的类
        var componentTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ComponentAttribute>() != null);

        foreach (var type in componentTypes)
        {
            // 创建实例并注册到容器中
            var instance = Activator.CreateInstance(type);
            if (instance != null)
            {
                m_Components[type] = instance;

                // 如果组件实现了 ILifecycle，则加入生命周期管理列表
                if (instance is ILifecycle lifecycleComponent)
                {
                    m_LifecycleComponents.Add(lifecycleComponent);
                }
            }
        }

        // 按优先级排序生命周期组件
        m_LifecycleComponents.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    // 从 XML 配置中注册组件
    public static void RegisterComponentsFromXml(string xmlFilePath)
    {
        var doc = XDocument.Load(xmlFilePath);
        if (doc == null || doc.Root == null)
        {
            return;
        }
        var components = doc.Root.Elements("component");

        foreach (var component in components)
        {
            var typeName = component.Attribute("type")?.Value;
            var name = component.Attribute("name")?.Value;

            if (string.IsNullOrEmpty(typeName))
            {
                throw new InvalidOperationException($"Component type '{typeName}' is missing in XML configuration.");
            }

            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new InvalidOperationException($"Type '{typeName}' not found.");
            }

            var instance = Activator.CreateInstance(type);

            if (!string.IsNullOrEmpty(name))
            {
                m_NamedComponents[name] = instance; // 按名称注册
            }
            else
            {
                m_Components[type] = instance; // 按类型注册
            }

            // 如果组件实现了 ILifecycle，则加入生命周期管理列表
            if (instance is ILifecycle lifecycleComponent)
            {
                m_LifecycleComponents.Add(lifecycleComponent);
            }
        }

        // 按优先级排序生命周期组件
        m_LifecycleComponents.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    // 自动注入 [Autowired] 标记的字段和属性
    public static void PerformDependencyInjection()
    {
        foreach (var component in m_Components.Values.Concat(m_NamedComponents.Values))
        {
            var componentType = component.GetType();

            // 注入字段
            var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<AutowiredAttribute>() != null);

            foreach (var field in fields)
            {
                var dependency = ResolveDependency(field.FieldType);
                field.SetValue(component, dependency);
            }

            // 注入属性
            var properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<AutowiredAttribute>() != null);

            foreach (var property in properties)
            {
                var dependency = ResolveDependency(property.PropertyType);
                property.SetValue(component, dependency);
            }
        }
    }

    // 解析依赖
    private static object ResolveDependency(Type type)
    {
        if (m_Components.TryGetValue(type, out var dependency))
        {
            return dependency;
        }

        throw new InvalidOperationException($"No component found for type {type.FullName}");
    }

    // 按名称解析依赖
    public static object ResolveDependencyByName(string name)
    {
        if (m_NamedComponents.TryGetValue(name, out var dependency))
        {
            return dependency;
        }

        throw new InvalidOperationException($"No component found with name '{name}'");
    }

    // 获取组件实例
    public static T GetComponent<T>()
    {
        return (T)m_Components[typeof(T)];
    }

    // 按名称获取组件实例
    public static object GetComponentByName(string name)
    {
        return m_NamedComponents[name];
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