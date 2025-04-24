namespace Lampyris.Server.Crypto.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// 根据 BarSize 对 DateTime 进行上取整
    /// </summary>
    public static DateTime Ceiling(this DateTime dateTime, BarSize barSize)
    {
        if (barSize == BarSize._1M)
        {
            // 特殊处理按月的上取整
            if (dateTime.Day == 1 && dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0)
            {
                return dateTime; // 已经是月初，无需调整
            }
            return new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1); // 下个月的月初
        }

        TimeSpan interval = GetInterval(barSize);
        long ticks = interval.Ticks;

        // 如果当前时间已经对齐，则直接返回
        if (dateTime.Ticks % ticks == 0)
        {
            return dateTime;
        }

        // 计算上取整时间
        long ceilingTicks = ((dateTime.Ticks + ticks - 1) / ticks) * ticks;
        return new DateTime(ceilingTicks, dateTime.Kind);
    }

    /// <summary>
    /// 根据 BarSize 对 DateTime 进行下取整
    /// </summary>
    public static DateTime Floor(this DateTime dateTime, BarSize barSize)
    {
        if (barSize == BarSize._1M)
        {
            // 特殊处理按月的下取整
            return new DateTime(dateTime.Year, dateTime.Month, 1); // 当前月的月初
        }

        TimeSpan interval = GetInterval(barSize);
        long ticks = interval.Ticks;

        // 计算下取整时间
        long floorTicks = (dateTime.Ticks / ticks) * ticks;
        return new DateTime(floorTicks, dateTime.Kind);
    }

    /// <summary>
    /// 根据指定分钟数对 DateTime 进行上取整
    /// </summary>
    public static DateTime Ceiling(this DateTime dateTime, int minutes)
    {
        if (minutes <= 0)
        {
            throw new ArgumentException("Minutes must be greater than zero.");
        }

        TimeSpan interval = TimeSpan.FromMinutes(minutes);
        long ticks = interval.Ticks;

        // 如果当前时间已经对齐，则直接返回
        if (dateTime.Ticks % ticks == 0)
        {
            return dateTime;
        }

        // 计算上取整时间
        long ceilingTicks = ((dateTime.Ticks + ticks - 1) / ticks) * ticks;
        return new DateTime(ceilingTicks, dateTime.Kind);
    }

    /// <summary>
    /// 根据指定分钟数对 DateTime 进行下取整
    /// </summary>
    public static DateTime Floor(this DateTime dateTime, int minutes)
    {
        if (minutes <= 0)
        {
            throw new ArgumentException("Minutes must be greater than zero.");
        }

        TimeSpan interval = TimeSpan.FromMinutes(minutes);
        long ticks = interval.Ticks;

        // 计算下取整时间
        long floorTicks = (dateTime.Ticks / ticks) * ticks;
        return new DateTime(floorTicks, dateTime.Kind);
    }

    /// <summary>
    /// 获取 BarSize 对应的时间间隔
    /// </summary>
    public static TimeSpan GetInterval(BarSize barSize)
    {
        return barSize switch
        {
            BarSize._1m => TimeSpan.FromMinutes(1),
            BarSize._3m => TimeSpan.FromMinutes(3),
            BarSize._5m => TimeSpan.FromMinutes(5),
            BarSize._15m => TimeSpan.FromMinutes(15),
            BarSize._30m => TimeSpan.FromMinutes(30),
            BarSize._1H => TimeSpan.FromHours(1),
            BarSize._2H => TimeSpan.FromHours(2),
            BarSize._4H => TimeSpan.FromHours(4),
            BarSize._6H => TimeSpan.FromHours(6),
            BarSize._12H => TimeSpan.FromHours(12),
            BarSize._1D => TimeSpan.FromDays(1),
            BarSize._3D => TimeSpan.FromDays(3),
            BarSize._1W => TimeSpan.FromDays(7),
            _ => throw new ArgumentException($"Unsupported BarSize: {barSize}")
        };
    }
}