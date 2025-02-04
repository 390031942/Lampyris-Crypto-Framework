namespace Lampyris.CSharp.Common;

public static class DateTimeUtil
{
    public static DateTime FromUnixTimestamp(long unixTimestampMilliseconds)
    {
        // Unix 纪元时间
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMilliseconds);
        // 转换为 DateTime 对象
        return dateTimeOffset.DateTime;
    }

    public static long ToUnixTimestampMilliseconds(DateTime dateTime)
    {
        // 将 DateTime 转换为 DateTimeOffset
        DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
        // 获取 Unix 时间戳（以毫秒为单位）
        return dateTimeOffset.ToUnixTimeMilliseconds();
    }

    public static long GetCurrentTimestamp()
    {
        return ToUnixTimestampMilliseconds(DateTime.Now);
    }
}
