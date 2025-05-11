namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  对k线数据流 监视条件 接口类
/// </summary>
public abstract class ICandleCondition
{
    /// <summary>
    /// K线时间间隔
    /// </summary>
    public BarSize BarSize { get; protected set; }

    /// <summary>
    /// 条件检测所需的k线数目
    /// </summary>
    public abstract int ExpectedCount { get; }
    
    public ICandleCondition(BarSize barSize)
    {
        BarSize = barSize;
    }
}   
