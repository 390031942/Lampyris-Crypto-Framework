using Lampyris.Crypto.Protocol.Quote;
using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component]
public class QuoteSubscriptionService
{
    [Autowired]
    private WebSocketService m_WebSocketService;

    #region Ticker行情订阅

    /// <summary>
    /// 订阅了Tickers数据的用户ID
    /// </summary>
    private HashSet<int> m_TickerSubscriptionUserIds = new HashSet<int>();

    /// <summary>
    /// ResSubscribeTickerData协议对象，避免重复创建
    /// </summary>
    private ResSubscribeTickerData m_ResSubscribeTickerData = new ResSubscribeTickerData();

    /// <summary>
    /// SymbolTickerDataBean对象池
    /// </summary>
    private ObjectPool<SymbolTickerDataBean> m_SymbolTickerDataBeanPool = new ObjectPool<SymbolTickerDataBean>();

    public bool HasSubscribedTicker(int clientUserId)
    {
        return m_TickerSubscriptionUserIds.Contains(clientUserId);
    }

    public bool SubscribeTicker(int clientUserId)
    {
        return m_TickerSubscriptionUserIds.Add(clientUserId);
    }

    public void PushTickerData(List<QuoteTickerData> quoteTickerDataList)
    {
        // 数据bean列表构造
        foreach (var quoteTickerData in quoteTickerDataList)
        {
            var bean = m_SymbolTickerDataBeanPool.Get();
            bean.Symbol = quoteTickerData.Symbol;
            bean.Price = (double)quoteTickerData.Price;
            bean.Percentage = (double)quoteTickerData.ChangePerc;
            bean.Currency = (double)quoteTickerData.Volumn;
            bean.MarkPrice = (double)quoteTickerData.MarkPrice;
            bean.IndexPrice = (double)quoteTickerData.IndexPrice;
            bean.FundingRate = (double)quoteTickerData.FundingRate;
            bean.NextFundingTime = quoteTickerData.NextFundingTime;
            bean.RiseSpeed = (double)quoteTickerData.RiseSpeed;
            bean.Labels.AddRange(quoteTickerData.Labels);

            m_ResSubscribeTickerData.BeanList.Add(bean);
        }

        // 发送
        m_WebSocketService.PushMessge(m_TickerSubscriptionUserIds, m_ResSubscribeTickerData);

        // 回收
        foreach (var bean in m_ResSubscribeTickerData.BeanList)
        {
            bean.Labels.Clear();
            m_SymbolTickerDataBeanPool.Recycle(bean);
        }
    }  
    #endregion

    #region K线数据订阅
    private class PerUserCandleDataSubscription
    {
        /// <summary>
        /// key   = Symbol 交易对
        /// value = 已经订阅了的barSize集合
        /// </summary>
        public Dictionary<string, HashSet<BarSize>> PerSymbolSubscriptionMap = new Dictionary<string, HashSet<BarSize>>();
    }

    private Dictionary<int, PerUserCandleDataSubscription> m_CandleDataSubscriptionMap = new Dictionary<int, PerUserCandleDataSubscription>();

    /// <summary>
    /// CandlestickBean对象池
    /// </summary>
    private ObjectPool<CandlestickBean> m_CandlestickBeanPool = new ObjectPool<CandlestickBean>();

    public void PushCandleDataList(int clientUserId, string symbol, BarSize barSize, List<QuoteCandleData> quoteCandleDataList)
    {
        ResCandlestickQuery res = new ResCandlestickQuery();
        res.BarSize = StringUtil.ToString(barSize);
        res.Symbol = symbol;

        foreach (var candleData in quoteCandleDataList)
        {
            CandlestickBean bean = ToCandlestickBean(candleData);
            res.BeanList.Add(bean);
        }

        m_WebSocketService.PushMessge(clientUserId, res);

        foreach (var bean in res.BeanList)
        {
            m_CandlestickBeanPool.Recycle(bean);
        }
    }

    private CandlestickBean ToCandlestickBean(QuoteCandleData candleData)
    {
        var bean = m_CandlestickBeanPool.Get();
        bean.High = candleData.High;
        bean.Low = candleData.Low;
        bean.Open = candleData.Open;
        bean.Close = candleData.Close;
        bean.Volume = candleData.Volume;
        bean.Currency = candleData.Currency;
        bean.Time = DateTimeUtil.ToUnixTimestampMilliseconds(candleData.DateTime);
        return bean;
    }

    public void PushCandleUpdateBean(int clientUserId, string symbol, BarSize barSize, QuoteCandleData quoteCandleData)
    {
        CandlestickUpdateBean bean = new CandlestickUpdateBean();
        bean.Symbol = symbol;
        bean.BarSize = StringUtil.ToString(barSize);
        bean.Bean = ToCandlestickBean(quoteCandleData);

        m_WebSocketService.PushMessge(clientUserId, bean);
    }
    #endregion

    #region Trade数据订阅
    /// <summary>
    /// key   = Symbol 交易对
    /// value = 已经订阅了的barSize集合
    /// </summary>
    private Dictionary<int, HashSet<string>> m_TradeDataSubscriptionMap = new Dictionary<int, HashSet<string>>();
    #endregion

    #region Symbol交易规则
    /// <summary>
    /// CandlestickBean对象池
    /// </summary>
    private ObjectPool<SymbolTradeRuleBean> m_SymbolTradeRuleDataPool = new ObjectPool<SymbolTradeRuleBean>();

    #endregion

    #region 异动数据

    /// <summary>
    /// MarketMonitorNoticeListBean协议对象，避免重复创建
    /// </summary>
    private MarketMonitorNoticeListBean m_MarketMonitorNoticeListBean = new MarketMonitorNoticeListBean();

    /// <summary>
    /// 推送市场异动信息(指定用户)
    /// </summary>
    public void PushMarketMonitorNotice(int clientUserId, List<MarketMonitorNoticeBean> noticeList)
    {
        MarketMonitorNoticeListBean noticeListBean = new MarketMonitorNoticeListBean
        {
            BeanList = { noticeList }
        };
        m_WebSocketService.PushMessge(clientUserId, noticeListBean);
    }

    /// <summary>
    /// 推送市场异动信息
    /// </summary>
    public void PushMarketMonitorNotice(List<MarketMonitorNoticeBean> noticeList)
    {
        MarketMonitorNoticeListBean noticeListBean = new MarketMonitorNoticeListBean
        {
            BeanList = { noticeList }
        };

        // 推送给所有订阅用户
        foreach (var userId in m_TickerSubscriptionUserIds)
        {
            m_WebSocketService.PushMessge(userId, noticeListBean);
        }
    }
    #endregion

    #region 自选信息数据

    private ObjectPool<SelfSelectedSymbolGroupBean> m_SelfSelectedSymbolGroupBeanPool = new ObjectPool<SelfSelectedSymbolGroupBean>();
    private ObjectPool<SelfSelectedSymbolInfoBean> m_SelfSelectedSymbolInfoBean = new ObjectPool<SelfSelectedSymbolInfoBean>();

    /// <summary>
    /// 推送用户的自选组
    /// </summary>
    /// <param name="clientUserId">用户ID</param>
    /// <param name="selfSelectedSymbolList">存储了一个组名列表，以及组名对应的自选数据列表</param>
    public void PushSelfSelectedSymbol(int clientUserId, List<KeyValuePair<SelfSelectedSymbolGroup,List<SelfSelectedSymbolData>>> selfSelectedSymbolList)
    {
        ResSelfSelectedSymbol res = new ResSelfSelectedSymbol();

        foreach(var groupData in selfSelectedSymbolList)
        {
            var symbolInfoList = groupData.Value;
            var bean = m_SelfSelectedSymbolGroupBeanPool.Get();

            bean.Name = groupData.Key.GroupName;
            bean.CanDelete = groupData.Key.CanDelete;

            foreach(var symbolInfo in symbolInfoList)
            {
                SelfSelectedSymbolInfoBean symbolInfoBean = m_SelfSelectedSymbolInfoBean.Get();
                symbolInfoBean.Symbol = symbolInfo.Symbol;
                symbolInfoBean.Timestamp = symbolInfo.Timestamp;
                symbolInfoBean.InitialPrice = symbolInfo.InitialPrice;

                bean.SymbolList.Add(symbolInfoBean);
            }
            res.GroupList.Add(bean);
        }

        // 推送数据
        m_WebSocketService.PushMessge(clientUserId, res);

        foreach(var bean in res.GroupList)
        {
            foreach(var symbolInfoBean in bean.SymbolList)
            {
                m_SelfSelectedSymbolInfoBean.Recycle(symbolInfoBean);
            }
            bean.SymbolList.Clear();
            m_SelfSelectedSymbolGroupBeanPool.Recycle(bean);
        }
    }

    internal bool CancelSubscribeTrade(int userId, string symbol)
    {
        throw new NotImplementedException();
    }

    internal bool SubscribeTrade(int userId, string symbol)
    {
        throw new NotImplementedException();
    }

    internal bool CancelSubscribeCandleUpdate(int userId, string symbol, BarSize barSize)
    {
        throw new NotImplementedException();
    }

    internal bool SubscribeCandleUpdate(int userId, string symbol, BarSize barSize)
    {
        throw new NotImplementedException();
    }
    #endregion
}
