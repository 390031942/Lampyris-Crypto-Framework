using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[IniFile("MarketMonitorSetting.ini")]
public static class MarketMonitorSetting
{
    [IniConfig("OneMinCandle", "1minK_SameColorCandleThreshold")]
    public static int OneMinSameColorCandleThreshold = 5;

    [IniConfig("OneMinCandle", "1minK_MA5_Threshold")]
    public static int OneMinMA5Threshold = 10;
}
