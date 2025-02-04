namespace Lampyris.Framework.Server.Common;

public static class Program
{
    private static int Main(string[] args)
    {
        LogManager.Instance.AddLogger(new ConsoleLogger());
        LogManager.Instance.AddLogger(new FileLogger("D:\\hji_okx_server.log"));
        
        LogManager.Instance.LogInfo("Starting server...");

        return Application.Instance.Run();
    }
}