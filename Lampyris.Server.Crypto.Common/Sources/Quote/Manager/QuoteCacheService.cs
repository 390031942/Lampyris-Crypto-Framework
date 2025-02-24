namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

[Component]
public class QuoteCacheService:ILifecycle
{
    private HashSet<string> m_allSymbolSet = new ();

    [Autowired]
    private DBService m_dbService;

    public override void OnStart()
    {
    }

    public List<QuoteCandleData> Query(string symbol, BarSize barSize, DateTime startTime, DateTime endTime)
    { 
        return new List<QuoteCandleData>();
    }

    public void Storage(string symbol, BarSize barSize, List<QuoteCandleData> dataList)
    {
        string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
        DBTable<QuoteCandleData> dbTable = m_dbService.GetTable<QuoteCandleData>(tableName);
        if(dbTable == null)
        {
            dbTable = m_dbService.CreateTable<QuoteCandleData>(tableName);
        }
        dbTable.Insert(dataList);
    }

    public QuoteCandleData QueryLastest(string symbol, BarSize okxBarSize)
    {
        return null;
    }

    public List<QuoteCandleData> QueryLastest(string symbol, BarSize okxBarSize, int n)
    {
        List<QuoteCandleData> result = new List<QuoteCandleData>();
        QueryLastestNoAlloc(symbol, okxBarSize, result, n);
        return result;
    }

    public void QueryLastestNoAlloc(string symbol, BarSize okxBarSize, List<QuoteCandleData> result, int n)
    {
        
    }

    public void Traversal(Action<string> foreachFunc)
    {
        if (foreachFunc == null)
            return;

        foreach (var symbol in m_allSymbolSet)
        {
            if(foreachFunc != null)
            {
                foreachFunc(symbol);
            }    
        }
    }
}
