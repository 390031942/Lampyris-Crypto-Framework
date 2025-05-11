namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 针对交易对的操作的冷却计时接口
/// </summary>
public abstract class ISymbolCoolDownChecker
{
    /// <summary>
    /// 冷却时间（毫秒）
    /// </summary>
    public abstract int CoolDownTime { get; }

    /// <summary>
    /// 上次触发的时间戳
    /// </summary>
    protected Dictionary<string, DateTime> m_LastTriggeredTimeMap = new Dictionary<string, DateTime>();

    public bool CheckCD(string symbol)
    {
        var LastTriggeredTime = m_LastTriggeredTimeMap.GetValueOrDefault(symbol);
        if(LastTriggeredTime == DateTime.MinValue || (DateTime.UtcNow - LastTriggeredTime).TotalMilliseconds <= CoolDownTime)
        {
            return true;
        }
        return false;
    }

    public void MarkCD(string symbol)
    {
        var LastTriggeredTime = DateTime.UtcNow;
        m_LastTriggeredTimeMap.Add(symbol, LastTriggeredTime);
    }
}
