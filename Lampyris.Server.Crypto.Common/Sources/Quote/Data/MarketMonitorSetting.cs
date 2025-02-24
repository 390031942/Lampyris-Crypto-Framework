using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[IniFile("market_monitor_setting.ini")]
public class MarketMonitorSetting
{
    [IniField("","")]
    public int OneMinSameColorCandleThreshold = 5;

    [IniField("","")]
    public int OneMinMA5Threshold = 10;
}
