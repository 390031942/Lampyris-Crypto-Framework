namespace Lampyris.Server.Crypto.Common;

public static class StringUtil
{
    /// <summary>
    /// 将 BarSize 枚举值转换为字符串类型（如 "1m"）。
    /// </summary>
    /// <param name="barSize">BarSize 枚举值</param>
    /// <returns>对应的字符串表示</returns>
    public static string ToString(BarSize barSize)
    {
        // 获取枚举值的名称，并去掉下划线
        return barSize.ToString().Replace("_", "");
    }
}
