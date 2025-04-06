namespace Lampyris.Server.Crypto.Common;

public static class DateTimeUtilEx
{
    public static double GetBarTimeSpanDiff(DateTime lhs, DateTime rhs, BarSize BarSize)
    {
        TimeSpan timeSpan = rhs - lhs;

        switch (BarSize)
        {
            case BarSize._1m:
                return (int)(timeSpan.TotalMinutes);
            case BarSize._3m:
                return (int)(timeSpan.TotalMinutes / 3);
            case BarSize._5m:
                return (int)(timeSpan.TotalMinutes / 5);
            case BarSize._15m:
                return (int)(timeSpan.TotalMinutes / 15);
            case BarSize._30m:
                return (int)(timeSpan.TotalMinutes / 30);
            case BarSize._1H:
                return (int)(timeSpan.TotalHours);
            case BarSize._2H:
                return (int)(timeSpan.TotalHours / 2);
            case BarSize._4H:
                return (int)(timeSpan.TotalHours / 4);
            case BarSize._6H:
                return (int)(timeSpan.TotalHours / 6);
            case BarSize._12H:
                return (int)(timeSpan.TotalHours / 12);
            case BarSize._1D:
                return (int)(timeSpan.TotalDays);
            case BarSize._3D:
                return (int)(timeSpan.TotalDays / 3);
            case BarSize._1W:
                return (int)(timeSpan.TotalDays / 7);
            case BarSize._1M:
                return Math.Abs(lhs.Year * lhs.Month - rhs.Year * rhs.Month);
        }

        return 0;
    }
}