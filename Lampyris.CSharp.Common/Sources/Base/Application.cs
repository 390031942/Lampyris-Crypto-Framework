namespace Lampyris.CSharp.Common;

using System.Reflection;

public static class Application
{
    private static IocContainerService ms_IocContainerService = new IocContainerService();

    // 运行标志
    private static bool ms_AppRunning = false;

    public static bool AppRunning => ms_AppRunning;

    static Application() 
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler((object? sender, EventArgs e) => {
            Quit();
        });
    }

    public static void Quit()
    {

    }

    public static int Run()
    {
        try
        {
            LoadAppConfig();
            LoadIocContainer();
        }
        catch (Exception ex)
        {
            // LogManager.Instance.LogError($"Uncaught exception:{ex.Message}\n\nStack trace:\n{ex.StackTrace}");
            return 1;
        }

        return 0;
    }

    private static void LoadAppConfig()
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
    private static void LoadIocContainer()
    {
        // 注册组件（通过扫描 [Component] 注解）
        ms_IocContainerService.RegisterComponents(Assembly.GetEntryAssembly());

        // 注册组件（通过 XML 配置）
        ms_IocContainerService.RegisterComponentsFromXml("components.xml");

        // 执行依赖注入
        ms_IocContainerService.PerformDependencyInjection();
    }
}