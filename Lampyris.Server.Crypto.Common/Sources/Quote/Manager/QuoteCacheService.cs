namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;

[Component]
public class QuoteCacheService:ILifecycle
{
    [Autowired]
    private readonly Logger m_logService;

    private HashSet<string> m_allSymbolSet = new ();

    public override void OnStart()
    {
        // 从文件中加载出来行情instId列表，并对每个instId逐个进行反序列化
        List<string> quoteList = SerializationManager.Instance.TryDeserializeObjectFromFile<List<string>>("quote-list.bin");
        List<string> okxBarSize = EnumNameManager.GetNames(typeof(BarSize));

        if(quoteList != null)
        {
            foreach (string instId in quoteList)
            {
                m_CandleDataMap[instId] = new Dictionary<BarSize, List<QuoteCandleData>>();
                foreach(var enumName in okxBarSize)
                {
                    BarSize barSize = (BarSize)EnumNameManager.GetEnum(typeof(BarSize), enumName);
                    string fileName = $"quote-cache/{instId}_{enumName}.bin";
                    var quoteCacheData = SerializationManager.Instance.TryDeserializeObjectFromFile<List<QuoteCandleData>>(fileName);
                    if (quoteCacheData != null)
                    {
                        m_CandleDataMap[instId][barSize] = quoteCacheData;
                    }
                }
            }
        }
    }

    public List<QuoteCandleData> Query(string instId, BarSize barSize, DateTime startTime, DateTime endTime)
    {
        return new List<QuoteCandleData>();
    }

    public void Storage(string symbol, BarSize barSize, List<QuoteCandleData> dataList)
    {
        if (string.IsNullOrEmpty(symbol)) 
        {
            m_logService.LogError("param \"instId\" can not be null or empty!");
            return;
        }

        if(dataList == null || dataList.Count <= 0)
        {
            m_logService.LogError("param \"dataList\" can not be null or empty!");
            return;

        } 

        if (!m_CandleDataMap.ContainsKey(instId))
        {
            m_CandleDataMap[instId] = new Dictionary<BarSize, List<QuoteCandleData>>();
        }

        var barSizeDataMap = m_CandleDataMap[instId];
        if(!barSizeDataMap.ContainsKey(barSize))
        {
            barSizeDataMap[barSize] = new List<QuoteCandleData>();
        }

        var storageList = barSizeDataMap[barSize];

        if (dataList != null)
        {
            int firstIndex = storageList.BinarySearch(dataList.First<QuoteCandleData>());

            // 开头都没找到，后面也不用找了
            if(firstIndex < 0)
            {
                // 直接找到第一个大于dataList.First<QuoteCandleData>().dateTime的索引
                int lowerIndex = storageList.LowerBound(dataList.First<QuoteCandleData>());

                // 在lowerIndex后插入
                storageList.InsertRange(lowerIndex, dataList);
            }
            else
            {
                int endIndex = storageList.BinarySearch(dataList.Last<QuoteCandleData>());
                // 末尾都没找到说明中间往后缺了一段数据
                if (endIndex < 0)
                {
                    // 直接找到第一个小于dataList.Last<QuoteCandleData>().dateTime的索引
                    int upperIndex = storageList.UpperBound(dataList.Last<QuoteCandleData>());

                    // 在upperIndex后插入
                    storageList.InsertRange(upperIndex, dataList);
                }
            }
        }
    }

    public QuoteCandleData QueryLastest(string instId, BarSize okxBarSize)
    {
        if(!m_CandleDataMap.ContainsKey(instId))
            return null;

        var barSizeDataMap = m_CandleDataMap[instId];

        if (!barSizeDataMap.ContainsKey(okxBarSize))
            return null;

        var storageList = barSizeDataMap[okxBarSize];
        if (storageList != null)
        {
            return storageList.Last<QuoteCandleData>();
        }

        return null;
    }

    public List<QuoteCandleData> QueryLastest(string instId, BarSize okxBarSize, int n)
    {
        List<QuoteCandleData> result = new List<QuoteCandleData>();
        QueryLastestNoAlloc(instId, okxBarSize, result, n);
        return result;
    }

    public void QueryLastestNoAlloc(string instId, BarSize okxBarSize, List<QuoteCandleData> result, int n)
    {
        result.Clear();
        if (!m_CandleDataMap.ContainsKey(instId))
            return;

        var barSizeDataMap = m_CandleDataMap[instId];
        if (!barSizeDataMap.ContainsKey(okxBarSize))
            return;

        var storageList = barSizeDataMap[okxBarSize];
        if (storageList != null)
        {
            for (int i = Math.Max(0, storageList.Count - n); i < storageList.Count; i++)
            {
                result.Add(storageList[i]);
            }
        }
    }

    public void Foreach(Action<string> foreachFunc)
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
