namespace Lampyris.CSharp.Common;

using System.Reflection;

public class Application:Singleton<Application>
{
    private IocContainerService m_IocContainerService = new IocContainerService();

    // 运行标志
    private bool m_AppRunning = false;

    public bool AppRunning => m_AppRunning;

    public Application() 
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler((object? sender, EventArgs e) => {
            Quit();
        });
    }
    private readonly List<BehaviourSingletonBase> m_InstanceList = new List<BehaviourSingletonBase>()
    {
        CallTimer.Instance,
        CoroutineManager.Instance,
    };

    public void Quit()
    {
        if (m_AppRunning)
        {
            m_AppRunning = false;
            for (int i = m_InstanceList.Count - 1; i >= 0; i--)
            {
                var behaviourSingletonBase = m_InstanceList[i];
                behaviourSingletonBase.OnDestroy();
            }
            SerializationManager.Instance.OnDestroy();
        }
    }

    public int Run()
    {
        try
        {
            LoadAppConfig();
            LoadIocContainer();

            foreach (var behaviourSingletonBase in m_InstanceList)
            {
                behaviourSingletonBase.OnStart();
            }
            SerializationManager.Instance.OnStart();

            m_AppRunning = true;
             
            long timestamp = 0;
            while (m_AppRunning)
            {
                long timestamp2 = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                long deltaTime = timestamp2 - timestamp;
                foreach (var behaviourSingletonBase in m_InstanceList)
                {
                    behaviourSingletonBase.OnUpdate(deltaTime);
                }
                timestamp = timestamp2;
            }
        }
        catch (Exception ex)
        {
            LogManager.Instance.LogError($"Uncaught exception:{ex.Message}\n\nStack trace:\n{ex.StackTrace}");
            return 1;
        }

        return 0;
    }

    private void LoadAppConfig()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        // 查找继承自 AppConfig 的非抽象类
        var appConfigTypes = types
            .Where(t => t.IsClass && !t.IsAbstract && typeof(AppConfig).IsAssignableFrom(t))
            .ToList();

        // 检查是否找到唯一的实现类
        if (appConfigTypes.Count == 0)
        {
            throw new InvalidOperationException("未找到 AppConfig 的实现类。");
        }
        else if (appConfigTypes.Count > 1)
        {
            throw new InvalidOperationException("找到多个 AppConfig 的实现类，请确保只有一个实现类。");
        }

        // 获取唯一的实现类
        var appConfigType = appConfigTypes.Single();

        // 创建实例
        var appConfigInstance = Activator.CreateInstance(appConfigType) as AppConfig;

        if (appConfigInstance != null)
        {
            // 打印 Name 和 Version
            Console.WriteLine($"Name: {appConfigInstance.Name}");
            Console.WriteLine($"Version: {appConfigInstance.Version}");
        }
    }
    private void LoadIocContainer()
    {
        // 注册组件（通过扫描 [Component] 注解）
        m_IocContainerService.RegisterComponents(Assembly.GetEntryAssembly());

        // 注册组件（通过 XML 配置）
        m_IocContainerService.RegisterComponentsFromXml("components.xml");

        // 执行依赖注入
        m_IocContainerService.PerformDependencyInjection();
    }
}