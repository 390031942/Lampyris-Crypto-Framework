namespace Lampyris.Server.Crypto.Common;

public interface IIndicator
{
    // 指标名称
    string Name { get; }

    // 计算指标
    List<double> Calculate(List<QuoteCandleData> data);

    // 查询某一段K线的指标
    List<double> Query(List<QuoteCandleData> data, DateTime startTime, DateTime endTime);
}