namespace Lampyris.Server.Crypto.Common;

/// <summary>
///  对k线数据流 监视条件 接口类
/// </summary>
public abstract class ICandleCondition: IDummyMarketAnomalyCondition
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

    /// <summary>
    /// 测试k线数据流是否满足条件
    /// </summary>
    /// <param name="dataList">k线列表只读容器</param>
    /// <param name="isEnd">是否当前barSize周期结束，如某时刻的59秒后会收到1min k线结束的k线数据，此时isEnd = true</param>
    /// <param name="value">异动数值</param>
    /// <returns></returns>
    public abstract bool Test(ReadOnlySpan<QuoteCandleData> dataList, bool isEnd, out decimal value);
}   
