namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;

[Component]
public class MarketDataService
{
    /// <summary>
    /// 全体USDT永续合约symbol
    /// </summary>
    private HashSet<string> m_AllSymbolSet = new HashSet<string>();

    public IReadOnlyCollection<string> GetAllSymbols()
    {
        return m_AllSymbolSet;
    }

    public void Init()
    {
        PlannedTaskScheduler.AddTimeTask()
    }

    public Task 
}
