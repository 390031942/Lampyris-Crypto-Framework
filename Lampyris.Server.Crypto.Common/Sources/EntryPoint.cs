﻿using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

public static class EntryPoint
{
    public static void RunMain(string[] args)
    {
        Logger.AddLogger(new ConsoleLogger());
        Logger.AddLogger(new FileLogger($"log_{DateTime.Now.ToString("yyyyMMddhhmmss")}.log"));
        Logger.LogInfo("Starting Server Application...");

        // 获取当前应用程序域中加载的所有程序集
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // 筛选名称包含关键字的程序集
        var matchingAssemblies = assemblies
            .Where(assembly => assembly.GetName().Name.Contains("Lampyris.Server.Crypto"))
            .ToList();

        Components.RegisterComponents(matchingAssemblies);
        Components.PerformDependencyInjection();
        // 注册退出事件，在程序退出前执行 OnDestroy
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            Logger.LogInfo("Program is exiting...");
            Components.DestroyLifecycle(); // 调用 OnDestroy
        };

        Logger.LogInfo("Program started.");
        try
        {
            Components.StartLifecycle(); // 调用 OnStart
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
            return;
        }

        // 循环执行 OnUpdate
        while (true)
        {
            Components.UpdateLifecycle(); // 调用 OnUpdate

            // 检测用户是否按下退出键（例如 Ctrl+C）
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
            {
                Logger.LogInfo("Exit key pressed. Exiting...");
                break; // 跳出循环，程序退出
            }
        }
    }
}