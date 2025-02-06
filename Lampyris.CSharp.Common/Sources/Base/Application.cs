namespace Lampyris.CSharp.Common;

using System.Reflection;

public static class Application
{
    private static IocContainerService ms_IocContainerService = new IocContainerService();

    // 运行标志
    private static bool ms_AppRunning = false;

    public static bool AppRunning => ms_AppRunning;

    public static AppConfig config { get; private set; }

    static Application() 
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        // 查找继承自 T 的非抽象类
        var targetType = types.FirstOrDefault(t => typeof(AppConfig).IsAssignableFrom(t) && !t.IsAbstract);

        if(targetType == null)
        {
            Logger.LogError("Cannot find implementation of \"AppConfig\", application will exit");
            Environment.Exit(-1);
        }

        config = Activator.CreateInstance(targetType) as AppConfig;

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
            LoadIocContainer();
            while (ms_AppRunning)
            {
                CoroutineManager.Tick();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Uncaught exception:{ex.Message}\n\nStack trace:\n{ex.StackTrace}");
            return 1;
        }

        return 0;
    }

    private static void LoadIocContainer()
    {
        // 注册组件（通过扫描 [Component] 注解）
        ms_IocContainerService.RegisterComponents(Assembly.GetExecutingAssembly());

        // 注册组件（通过 XML 配置）
        ms_IocContainerService.RegisterComponentsFromXml("components.xml");

        // 执行依赖注入
        ms_IocContainerService.PerformDependencyInjection();
    }
}