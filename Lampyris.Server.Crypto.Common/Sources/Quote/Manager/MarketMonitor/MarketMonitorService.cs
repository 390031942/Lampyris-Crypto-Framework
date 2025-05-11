namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.ComponentModel;

[Component]
public class MarketMonitorService:ILifecycle
{
    [Autowired]
    private AbstractQuoteProviderService m_QuoteProviderService;

    public override void OnStart()
    {
        m_QuoteProviderService.OnTickerUpdated += TestTickerData;
        m_QuoteProviderService.OnCandleDataUpdated += TestCandleData;
    }

    public void TestTickerData(IEnumerable<QuoteTickerData> dataList)
    {

    }

    public void TestCandleData(string symbol, BarSize barSize, bool isEnd)
    {

    }
}
