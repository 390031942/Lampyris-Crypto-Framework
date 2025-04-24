namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

[Component]
public class QuoteCacheService:ILifecycle
{
    [Autowired]
    private DBService m_DBService;

    private ICacheService m_CacheService;

    public override void OnStart()
    {
        m_CacheService = CacheServiceFactory.Get(CacheServiceType.MemoryCache);
    }

    /// <summary>
    /// 查询指定时间范围内的k线列表
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="startTime">开始的时间范围</param>
    /// <param name="endTime">结束的时间范围</param>
    /// <returns></returns>
    public List<QuoteCandleData> QueryCandleData(string symbol, BarSize barSize, DateTime startTime, DateTime endTime)
    {
        List<QuoteCandleData> result = new List<QuoteCandleData>();

        // 生成缓存键
        string cacheKey = $"{symbol}_{barSize}_candles";
        if (m_CacheService.ContainsKey(cacheKey))
        {
            // 从缓存中获取数据
            var cachedData = m_CacheService.Get<List<QuoteCandleData>>(cacheKey);
            if(cachedData != null)
            {
                result.AddRange(cachedData.Where(c => c.DateTime >= startTime && c.DateTime <= endTime));
            }
        }

        // 如果缓存中没有数据或数据不足，从数据库中查询
        if (result.Count == 0)
        {
            string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
            DBTable<QuoteCandleData> dbTable = m_DBService.GetTable<QuoteCandleData>(tableName);
            if (dbTable != null)
            {
                var dbData = dbTable.Query(queryCondition:"DateTime BETWEEN @StartDate AND @EndDate",
                                           parameters:    SQLParamMaker.Begin()
                                                          .Append("StartDate", startTime)
                                                          .Append("EndDate  ", endTime)
                                                          .End(),
                                           orderBy:       "DateTime",
                                           ascending:     false);
                result.AddRange(dbData);
            }
        }

        return result;
    }

    // 对于符合缓存策略的数据，可以选择同步写入到缓存里
    // 缓存策略为:
    // 1) 对于barSize = 1m的，缓存最近7天的数据，数据按日期分组
    // 2) 对于barSize = 15m的，缓存最近14天的数据，数据按日期分组
    // 3) 对于barSize = 1d的，缓存全体数据，数据不分组
    // 其余情况下不写入缓存

    /// <summary>
    /// 存储K线数据到缓存+数据库
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="dataList">k线数据列表</param>
    public void StorageCandleData(string symbol, BarSize barSize, List<QuoteCandleData> dataList, bool cached = false)
    {
        // TODO:异步写入到数据库
        string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
        DBTable<QuoteCandleData> dbTable = m_DBService.GetTable<QuoteCandleData>(tableName);
        if(dbTable == null)
        {
            dbTable = m_DBService.CreateTable<QuoteCandleData>(tableName);
        }
        dbTable.Insert(dataList);
    }

    /// <summary>
    /// 存储K线数据到缓存+数据库
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="dataList">k线数据</param>
    public void StorageCandleData(string symbol, BarSize barSize, QuoteCandleData data)
    {
        // TODO:异步写入到数据库
        string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
        DBTable<QuoteCandleData> dbTable = m_DBService.GetTable<QuoteCandleData>(tableName);
        if (dbTable == null)
        {
            dbTable = m_DBService.CreateTable<QuoteCandleData>(tableName);
        }
        dbTable.Insert(data);
    }


    /// <summary>
    /// 查询最近一根k线
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol<</param>
    /// <param name="okxBarSize">k线图时间周期</param>
    /// <returns></returns>
    public QuoteCandleData QueryLastestCandle(string symbol, BarSize okxBarSize)
    {
        return null;
    }

    /// <summary>
    /// 查询最近n根k线
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol<</param>
    /// <param name="okxBarSize">k线图时间周期</param>
    /// <param name="n">需要返回的数量,如果数据总数小于n,则会返回当前所有的数据</param>
    /// <returns></returns>
    public List<QuoteCandleData> QueryLastestCandles(string symbol, BarSize okxBarSize, int n)
    {
        List<QuoteCandleData> result = new List<QuoteCandleData>();
        QueryLastestCandlesNoAlloc(symbol, okxBarSize, result, n);
        return result;
    }

    /// <summary>
    /// 查询最近n根k线(复用列表对象)
    /// </summary>
    /// <returns></returns>
    public void QueryLastestCandlesNoAlloc(string symbol, BarSize okxBarSize, List<QuoteCandleData> result, int n)
    {
        
    }

    /// <summary>
    /// 查询某个symbol，对于某个时间周期下，数据库中存储了的全体日期列表
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    /// <returns></returns>
    public IEnumerable<DateTime> QueryCandleDateTimeList(string symbol, BarSize barSize)
    {
        string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
        return m_DBService.QueryField<DateTime>(tableName, "dateTime");
    }
}
