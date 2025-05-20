namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

/// <summary>
/// K线数据缓存策略
/// </summary>
public class QuoteCandleDataCacheStrategy
{
    /// <summary>
    /// 缓存k线数据的时间间隔
    /// </summary>
    public BarSize BarSize { get; private set; }

    /// <summary>
    /// 需要缓存的K线数据的天数，如果为-1，则表示缓存每一天的是数据
    /// </summary>
    public int Day;

    /// <summary>
    /// 每个缓存数据分组的时间间隔秒数,如1min k线的缓存是1天一组，则秒数为24*3600 = 86400.
    /// 如果值等于非正数，说明数据缓存全部数据，而不分组
    /// </summary>
    public int PerGroupIntervalSec {get; private set;}

    public QuoteCandleDataCacheStrategy(BarSize barSize, int day, int perGroupIntervalSec)
    {
        this.BarSize = barSize;
        this.Day = day;
        this.PerGroupIntervalSec = perGroupIntervalSec;
    }
    
    /// <summary>
    /// 构建从缓存中获取数据的key
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="dateTime">取k线数据的参考时间,用于确定分组</param>
    /// <returns></returns>
    public string MakeCacheKey(string symbol,DateTime dateTime)
    {
        if (PerGroupIntervalSec <= 0)
        {
            return $"candle_{symbol}_{BarSize}";
        }

        long timestampSec = DateTimeUtil.ToUnixTimestampMilliseconds(dateTime);
        // 对时间戳取模
        long groupId = timestampSec / PerGroupIntervalSec;

        return $"{symbol}_{BarSize}_{groupId}";
    }

    /// <summary>
    /// 构建从缓存中获取数据的key
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <param name="startTime">时间范围的起始时间</param>
    /// <param name="endTime">时间范围的结束时间</param>
    /// <returns>返回生成的缓存键集合</returns>
    public List<string> MakeCacheKey(string symbol, DateTime startTime, DateTime endTime, List<string>? preAllocated = null)
    {
        List<string> keys = preAllocated ?? new List<string>();
        // 如果时间范围无效，直接返回空列表
        if (endTime < startTime)
        {
            return keys;
        }

        // 如果 PerGroupIntervalSec <= 0，返回单一的缓存键
        if (PerGroupIntervalSec <= 0)
        {
            keys.Add($"candle_{symbol}_{BarSize}");
            return keys;
        }

        // 将起始时间和结束时间转换为 Unix 时间戳（毫秒）
        long startTimestampSec = DateTimeUtil.ToUnixTimestampMilliseconds(startTime);
        long endTimestampSec = DateTimeUtil.ToUnixTimestampMilliseconds(endTime);

        // 计算第一个分组 ID
        long currentGroupId = startTimestampSec / PerGroupIntervalSec;

        // 遍历每个分组，直到结束时间
        while (currentGroupId * PerGroupIntervalSec <= endTimestampSec)
        {
            // 生成缓存键并添加到列表
            keys.Add($"{symbol}_{BarSize}_{currentGroupId}");

            // 移动到下一个分组
            currentGroupId++;
        }

        return keys;
    }

    /// <summary>
    /// 判断两个时间点对应的groupId是否相同，如果相同，它们对应的cacheKey也相同
    /// </summary>
    /// <param name="lhs">第一个时间点</param>
    /// <param name="rhs">第二个时间点</param>
    /// <returns>如果两个时间点属于同一个分组，返回 true；否则返回 false</returns>
    public bool IsInSameGroup(DateTime lhs, DateTime rhs)
    {
        // 如果 PerGroupIntervalSec <= 0，认为所有时间点都属于同一个分组
        if (PerGroupIntervalSec <= 0)
        {
            return true;
        }

        // 将时间点转换为 Unix 时间戳（毫秒）
        long lhsTimestamp = DateTimeUtil.ToUnixTimestampMilliseconds(lhs);
        long rhsTimestamp = DateTimeUtil.ToUnixTimestampMilliseconds(rhs);

        // 计算两个时间点的 groupId
        long lhsGroupId = lhsTimestamp / PerGroupIntervalSec;
        long rhsGroupId = rhsTimestamp / PerGroupIntervalSec;

        // 判断两个 groupId 是否相同
        return lhsGroupId == rhsGroupId;
    }

    public DateTime? CalculateCacheStartDateTime(DateTime now)
    {
        DateTime dateTime = now;
        if(Day > 0)
        {
            dateTime = dateTime.AddDays(-Day);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        return null;
    }
}

[Component]
public class QuoteCacheService:ILifecycle
{
    [Autowired]
    private QuoteDBService m_DBService;

    private ICacheService m_CacheService;

    private List<QuoteCandleDataCacheStrategy> m_CacheStrategyList = new List<QuoteCandleDataCacheStrategy>();
    private Dictionary<BarSize, QuoteCandleDataCacheStrategy> m_CacheStrategyMap = new Dictionary<BarSize, QuoteCandleDataCacheStrategy>();

    // 缓存所有symbol的上架时间
    private Dictionary<string, DateTime> m_OnBoardDateTimeMap = new Dictionary<string, DateTime>();

    private ObjectListPool<string> m_StringListPool = new ObjectListPool<string>();

    public QuoteCacheService()
    {
        m_CacheService = CacheServiceFactory.Get(CacheServiceType.MemoryCache);
        m_CacheStrategyList.Add(new QuoteCandleDataCacheStrategy(BarSize._1m, 7, 24 * 60 * 60));
        m_CacheStrategyList.Add(new QuoteCandleDataCacheStrategy(BarSize._15m, 14,24 * 60 * 60));
        m_CacheStrategyList.Add(new QuoteCandleDataCacheStrategy(BarSize._1D, -1, -1));

        foreach(var strategy in m_CacheStrategyList)
        {
            m_CacheStrategyMap[strategy.BarSize] = strategy;
        }
    }

    /// <summary>
    /// 更新上架时间
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="date"></param>
    public void UpdateOnBoardDateTime(string symbol, DateTime date)
    {
        m_OnBoardDateTimeMap.Add(symbol, date);
    }

    /// <summary>
    /// 查询上架时间
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public DateTime GetOnBoardDateTime(string symbol)
    {
        return m_OnBoardDateTimeMap.ContainsKey(symbol) ? m_OnBoardDateTimeMap[symbol]: DateTime.MinValue;
    }

    /// <summary>
    /// 获取缓存策略
    /// </summary>
    /// <returns></returns>
    public ReadOnlyCollection<QuoteCandleDataCacheStrategy> GetCacheStrategies()
    {
        return m_CacheStrategyList.AsReadOnly();
    }

    /// <summary>
    /// 查询指定时间范围内的k线列表
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="startTime">开始的时间范围</param>
    /// <param name="endTime">结束的时间范围</param>
    /// <param name="n">最多返回的k线数量，-1表示返回全部</param>
    /// <returns>返回符合条件的k线数据列表</returns>
    public List<QuoteCandleData> QueryCandleData(string symbol, BarSize barSize, DateTime? startTime, DateTime? endTime, int n = -1)
    {
        if (!m_CacheStrategyMap.TryGetValue(barSize, out var strategy))
        {
            return null;
        }

        if (!m_OnBoardDateTimeMap.TryGetValue(symbol, out DateTime onBoardTime))
        {
            return null;
        }

        List<QuoteCandleData> result = new List<QuoteCandleData>();

        // 生成缓存键
        if (startTime == null)
        {
            startTime = onBoardTime;
        }

        if (endTime == null)
        {
            endTime = DateTimeUtilEx.GetServerDateTime();
        }

        List<string> cacheKeyList = strategy.MakeCacheKey(symbol, startTime.Value, endTime.Value, m_StringListPool.Get());

        foreach (string cacheKey in cacheKeyList)
        {
            if (m_CacheService.ContainsKey(cacheKey))
            {
                // 从缓存中获取数据
                var cachedData = m_CacheService.Get<List<QuoteCandleData>>(cacheKey);
                if (cachedData != null)
                {
                    // 根据时间范围过滤数据
                    var filteredData = cachedData.Where(c =>
                        (!startTime.HasValue || c.DateTime >= startTime.Value) &&
                        (!endTime.HasValue || c.DateTime <= endTime.Value));

                    // 如果 n > 0，限制返回的数量
                    if (n > 0)
                    {
                        result.AddRange(filteredData.Take(n - result.Count));
                        if (result.Count >= n)
                        {
                            break; // 已达到 n 条数据，停止处理
                        }
                    }
                    else
                    {
                        result.AddRange(filteredData);
                    }
                }
            }
        }

        m_StringListPool.Recycle(cacheKeyList);

        // 如果缓存中没有数据或数据不足，从数据库中查询
        if (result.Count == 0 || (n > 0 && result.Count < n))
        {
            string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
            DBTable<QuoteCandleData> dbTable = m_DBService.GetTable<QuoteCandleData>(tableName);
            if (dbTable != null)
            {
                // 构建查询条件
                string queryCondition = BuildQueryCondition(startTime, endTime);

                // 构建查询参数
                var parameters = SQLParamMaker.Begin();
                if (startTime.HasValue)
                    parameters.Append("StartDate", startTime.Value);
                if (endTime.HasValue)
                    parameters.Append("EndDate", endTime.Value);

                // 执行查询
                var dbData = dbTable.Query(
                    queryCondition: queryCondition,
                    parameters: parameters.End(),
                    orderBy: "DateTime",
                    ascending: false);

                // 如果 n > 0，限制返回的数量
                if (n > 0)
                {
                    result.AddRange(dbData.Take(n - result.Count));
                }
                else
                {
                    result.AddRange(dbData);
                }
            }
        }

        return result;
    }


    /// <summary>
    /// 查询指定时间范围内的k线列表(只返回Span，仅仅查询缓存中的数据，避免新分配容器)
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="startTime">开始的时间范围</param>
    /// <param name="endTime">结束的时间范围</param>
    /// <param name="n">最多返回的k线数量，-1表示返回全部</param>
    /// <returns>返回符合条件的k线数据列表</returns>
    public ReadOnlySpan<QuoteCandleData> QueryCacheOnlyCandleData(string symbol, BarSize barSize, DateTime? startTime, DateTime? endTime, int n = -1)
    {
        // 检查是否有对应的缓存策略
        if (!m_CacheStrategyMap.TryGetValue(barSize, out var strategy))
        {
            return ReadOnlySpan<QuoteCandleData>.Empty;
        }

        // 检查是否有对应的上架时间
        if (!m_OnBoardDateTimeMap.TryGetValue(symbol, out DateTime onBoardTime))
        {
            return ReadOnlySpan<QuoteCandleData>.Empty;
        }

        // 如果 startTime 为 null，则使用上架时间
        if (startTime == null)
        {
            startTime = onBoardTime;
        }

        // 如果 endTime 为 null，则使用当前服务器时间
        if (endTime == null)
        {
            endTime = DateTimeUtilEx.GetServerDateTime();
        }

        // 生成缓存键列表
        List<string> cacheKeyList = strategy.MakeCacheKey(symbol, startTime.Value, endTime.Value, m_StringListPool.Get());

        foreach (string cacheKey in cacheKeyList)
        {
            if (m_CacheService.ContainsKey(cacheKey))
            {
                // 从缓存中获取数据
                var cachedData = m_CacheService.Get<List<QuoteCandleData>>(cacheKey);
                if (cachedData != null)
                {
                    // 遍历缓存数据，找到符合条件的范围
                    int startIndex = -1;
                    int endIndex = -1;

                    for (int i = 0; i < cachedData.Count; i++)
                    {
                        var candle = cachedData[i];
                        if (startIndex == -1 && candle.DateTime >= startTime.Value)
                        {
                            startIndex = i;
                        }

                        if (candle.DateTime > endTime.Value)
                        {
                            endIndex = i;
                            break;
                        }
                    }

                    // 如果找到符合条件的范围，返回 Span
                    if (startIndex != -1)
                    {
                        if (endIndex == -1)
                        {
                            endIndex = cachedData.Count; // 如果没有找到结束索引，取到最后
                        }

                        // 计算返回的数量
                        int count = endIndex - startIndex;
                        if (n != -1 && count > n)
                        {
                            count = n; // 限制返回的数量为 n
                        }

                        m_StringListPool.Recycle(cacheKeyList);
                        return CollectionsMarshal.AsSpan(cachedData).Slice(startIndex, count);
                    }
                }
            }
        }

        // 回收缓存键列表
        m_StringListPool.Recycle(cacheKeyList);

        // 如果没有找到符合条件的数据，返回空 Span
        return ReadOnlySpan<QuoteCandleData>.Empty;
    }



    /// <summary>
    /// 构建查询条件
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>SQL 查询条件字符串</returns>
    private string BuildQueryCondition(DateTime? startTime, DateTime? endTime)
    {
        if (startTime.HasValue && endTime.HasValue)
        {
            return "DateTime BETWEEN @StartDate AND @EndDate";
        }
        else if (startTime.HasValue)
        {
            return "DateTime >= @StartDate";
        }
        else if (endTime.HasValue)
        {
            return "DateTime <= @EndDate";
        }
        else
        {
            return "1 = 1"; // 查询所有数据
        }
    }

    // 对于符合缓存策略的数据，可以选择同步写入到缓存里
    // 缓存策略为:
    // 1) 对于barSize = 1m的，缓存最近7天的数据，数据按日期分组
    // 2) 对于barSize = 15m的，缓存最近14天的数据，数据按日期分组
    // 3) 对于barSize = 1d的，缓存全体数据，数据不分组
    // 其余情况下不写入缓存

    /// <summary>
    /// 存储"一根"K线数据到缓存+数据库
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="dataList">k线数据列表</param>
    public void StorageCandleData(string symbol, BarSize barSize, QuoteCandleData data, bool cached = false)
    {
        if(data == null)
        {
            return;
        }

        // 缓存写入
        string cacheKey = $"{symbol}_{barSize}_candles";

        if (!m_CacheService.ContainsKey(cacheKey))
        {
            m_CacheService.Set(cacheKey, new List<QuoteCandleData>());
        }
        List<QuoteCandleData> cachedList = m_CacheService.Get<List<QuoteCandleData>>(cacheKey) ?? new List<QuoteCandleData>();
        cachedList.Add(data);
        cachedList.Sort((a, b) => a.DateTime.CompareTo(b.DateTime));

        // 异步DB写入
        TaskManager.RunTask($"DB Write QuoteCandleData Symbol = {symbol}, barSize = {barSize}", 0, (progress, token) =>
        {
            progress.Percentage = 0;
            string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
            DBTable<QuoteCandleData> dbTable = m_DBService.GetTable<QuoteCandleData>(tableName);
            if (dbTable == null)
            {
                dbTable = m_DBService.CreateTable<QuoteCandleData>(tableName);
            }
            dbTable.Insert(data);
            progress.Percentage = 100;
        });
    }

    /// <summary>
    /// 存储"K线列表"数据到缓存+数据库
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="dataList">k线数据列表</param>
    public void StorageCandleData(string symbol, BarSize barSize, List<QuoteCandleData> dataList, bool cached = false)
    {
        if (dataList == null || dataList.Count <= 0)
        {
            return;
        }

        // 检查是否有对应的缓存策略
        if (m_CacheStrategyMap.TryGetValue(barSize, out var strategy))
        {
            // 缓存写入
            string cacheKey = strategy.MakeCacheKey(symbol, dataList[0].DateTime);
            if (!m_CacheService.ContainsKey(cacheKey))
            {
                m_CacheService.Set(cacheKey, new List<QuoteCandleData>());
            }
            List<QuoteCandleData> cachedList = m_CacheService.Get<List<QuoteCandleData>>(cacheKey) ?? new List<QuoteCandleData>();
            for(int i = 0; i < dataList.Count; i++) 
            {
                QuoteCandleData data = dataList[i];
                if (i > 0 && !strategy.IsInSameGroup(dataList[i - 1].DateTime, data.DateTime))
                {
                    cacheKey = strategy.MakeCacheKey(symbol, dataList[i].DateTime);
                    cachedList.Sort((a,b) => a.DateTime.CompareTo(b.DateTime));

                    if (!m_CacheService.ContainsKey(cacheKey))
                    {
                        m_CacheService.Set(cacheKey, new List<QuoteCandleData>());
                    }
                    cachedList = m_CacheService.Get<List<QuoteCandleData>>(cacheKey) ?? new List<QuoteCandleData>();
                }
                cachedList.Add(data);
                m_CacheService.Set(cacheKey, data);
            }
            cachedList.Sort((a, b) => a.DateTime.CompareTo(b.DateTime));
        }

        // 异步DB写入
        TaskManager.RunTask($"DB Write QuoteCandleData Symbol = {symbol}, barSize = {barSize}, size = {dataList.Count}", 0, (progress, token) =>
        {
            progress.Percentage = 0;
            string tableName = $"quote_candle_data_{symbol}{barSize.ToString()}";
            DBTable<QuoteCandleData> dbTable = m_DBService.GetTable<QuoteCandleData>(tableName);
            if (dbTable == null)
            {
                dbTable = m_DBService.CreateTable<QuoteCandleData>(tableName);
            }
            dbTable.Insert(dataList, true);
            progress.Percentage = 100;
        });
    }

    /// <summary>
    /// 查询最近一根k线
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol<</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <returns></returns>
    public QuoteCandleData QueryLastestCandle(string symbol, BarSize barSize)
    {
        return null;
    }

    /// <summary>
    /// 查询最近n根k线(只从缓存中找)
    /// </summary>
    /// <param name="symbol">USDT永续合约symbol</param>
    /// <param name="barSize">k线图时间周期</param>
    /// <param name="n">需要的最近k线数量</param>
    /// <returns>返回最近n根k线的只读视图</returns>
    public ReadOnlySpan<QuoteCandleData> QueryCacheOnlyLastestCandles(string symbol, BarSize barSize, int n)
    {
        // 检查是否有对应的缓存策略
        if (!m_CacheStrategyMap.TryGetValue(barSize, out var strategy))
        {
            return ReadOnlySpan<QuoteCandleData>.Empty;
        }

        // 检查是否有对应的上架时间
        if (!m_OnBoardDateTimeMap.TryGetValue(symbol, out DateTime onBoardTime))
        {
            return ReadOnlySpan<QuoteCandleData>.Empty;
        }

        // 获取当前时间，生成缓存键列表
        DateTime endTime = DateTimeUtilEx.GetServerDateTime();
        List<string> cacheKeyList = strategy.MakeCacheKey(symbol, onBoardTime, endTime, m_StringListPool.Get());

        // 临时变量，用于存储最新的缓存数据
        Span<QuoteCandleData> latestSpan = default;

        // 遍历缓存键列表，从最新的缓存中查找数据
        for (int i = cacheKeyList.Count - 1; i >= 0; i--) // 从最新的缓存键开始
        {
            string cacheKey = cacheKeyList[i];
            if (m_CacheService.ContainsKey(cacheKey))
            {
                // 从缓存中获取数据
                var cachedData = m_CacheService.Get<List<QuoteCandleData>>(cacheKey);
                if (cachedData != null && cachedData.Count > 0)
                {
                    // 使用 CollectionsMarshal.AsSpan 获取 Span
                    var cachedSpan = CollectionsMarshal.AsSpan(cachedData);

                    // 如果当前缓存数据足够多，直接返回最近 n 根
                    if (cachedSpan.Length >= n)
                    {
                        m_StringListPool.Recycle(cacheKeyList);
                        return cachedSpan.Slice(cachedSpan.Length - n, n);
                    }

                    // 如果当前缓存数据不足 n 根，记录下来，继续查找
                    latestSpan = cachedSpan;
                    n -= cachedSpan.Length;
                }
            }
        }

        // 回收缓存键列表
        m_StringListPool.Recycle(cacheKeyList);

        // 如果没有足够的数据，返回已找到的部分
        return latestSpan;
    }


    /// <summary>
    /// 查询某个symbol，对于某个时间周期下，数据库中存储了的全体日期列表
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="barSize"></param>
    /// <returns></returns>
    public IEnumerable<DateTime> QueryCandleDateTimeList(string symbol, BarSize barSize)
    {
        string tableName = $"quote_candle_data_{symbol}{barSize}";
        if(!m_DBService.TableExists(tableName))
        {
            return Enumerable.Empty<DateTime>();
        }
        var table = m_DBService.GetTable<QuoteCandleData>(tableName);
        return table.QueryField<DateTime>("dateTime");
    }
}
