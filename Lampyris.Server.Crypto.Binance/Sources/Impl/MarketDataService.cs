namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;

[Component]
public class MarketDataService
{
    [Autowired]
    private ProxyProvideService m_ProxyProvideService;

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
        MarketDataWebSocketClient webSocketClient = new MarketDataWebSocketClient(m_ProxyProvideService.);

    }
}
