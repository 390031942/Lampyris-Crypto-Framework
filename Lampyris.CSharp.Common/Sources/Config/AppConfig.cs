namespace Lampyris.CSharp.Common;

/*
 * 全局程序配置，在程序集中必须要有一个AppConfig的实现类，否则无法启动程序
 */
public abstract class AppConfig
{
    public abstract string Name { get; }
    public abstract string Version { get; }
}