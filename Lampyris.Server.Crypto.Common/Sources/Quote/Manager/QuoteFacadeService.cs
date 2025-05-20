using Lampyris.Crypto.Protocol.App;
using Lampyris.Crypto.Protocol.Common;
using Lampyris.Crypto.Protocol.Quote;
using Lampyris.CSharp.Common;
using System.Text;

namespace Lampyris.Server.Crypto.Common;

[Component(tag:"MessageHandler")]
public class QuoteFacadeService
{
    [Autowired]
    private QuoteSubscriptionService m_QuoteSubscriptionService;

    [Autowired]
    private AbstractQuoteProviderService m_QuoteProviderService;

    [Autowired]
    private SelfSelectSymbolService m_SelfSelectSymbolService;

    [Autowired]
    private WebSocketService m_WebSocketService;

    #region Ticker订阅
    [MessageHandler(Request.RequestTypeOneofCase.ReqSubscribeTickerData)]
    public void ReqSubscribeTickerData(ClientUserInfo clientUserInfo, Request request)
    {
        ReqSubscribeTickerData req = request.ReqSubscribeTickerData;
        bool success = m_QuoteSubscriptionService.SubscribeTicker(clientUserInfo.UserId);
        if (!success)
        {
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = "订阅Ticker数据失败:已订阅无法重新订阅",
                Type = NoticeType.Toast,
            });
        }
    }
    #endregion

    [MessageHandler(Request.RequestTypeOneofCase.ReqCandlestickQuery)]
    public void ReqCandlestickQuery(ClientUserInfo clientUserInfo, Request request)
    {
        ReqCandlestickQuery reqCandlestickQuery = request.ReqCandlestickQuery;
        if (!Enum.TryParse(reqCandlestickQuery.BarSize, ignoreCase: false, out BarSize barSize))
        {
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = $"查询k线数据失败:无法识别的barSize参数:{reqCandlestickQuery.BarSize}",
                Type = NoticeType.Toast,
            });
        }

        DateTime? startTime = null;
        DateTime? endTime = null;

        if (reqCandlestickQuery.StartTime > 0)
        {
            startTime = DateTimeUtil.FromUnixTimestamp(reqCandlestickQuery.StartTime);
        }
        if (reqCandlestickQuery.EndTime > 0)
        {
            startTime = DateTimeUtil.FromUnixTimestamp(reqCandlestickQuery.EndTime);
        }

        var span = m_QuoteProviderService.QueryCandleData(reqCandlestickQuery.Symbol, barSize, startTime, endTime);
        if (span != null && !span.IsEmpty)
        {
            ResCandlestickQuery resCandlestickQuery = new ResCandlestickQuery();
            resCandlestickQuery.Symbol = reqCandlestickQuery.Symbol;
            resCandlestickQuery.BarSize = reqCandlestickQuery.BarSize;
            foreach (var candleData in span)
            {
                resCandlestickQuery.BeanList.Add(ToCandleBean(candleData));
            }

            m_WebSocketService.PushMessge(clientUserInfo.UserId, resCandlestickQuery);
        }
        else
        {
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = $"查询k线数据失败:k线数据为空",
                Type = NoticeType.Toast,
            });
        }
    }

    private static CandlestickBean ToCandleBean(QuoteCandleData candleData)
    {
        return new CandlestickBean()
        {
            Open = candleData.Open,
            Close = candleData.Close,
            High = candleData.High,
            Low = candleData.Low,
            Volume = candleData.Volume,
            Currency = candleData.Currency,
            Time = DateTimeUtil.ToUnixTimestampMilliseconds(candleData.DateTime),
        };
    }

    [MessageHandler(Request.RequestTypeOneofCase.ReqSubscribeTradeData)]
    public void ReqSubscribeTradeData(ClientUserInfo clientUserInfo, Request request)
    {
        ReqSubscribeTradeData reqSubscribeTradeData = request.ReqSubscribeTradeData;
        if (reqSubscribeTradeData.Symbols.Count == 0)
        {
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = $"订阅Trade数据失败:symbol列表为空",
                Type = NoticeType.Toast,
            });
        }

        StringBuilder sb = new StringBuilder();
        foreach (var symbol in reqSubscribeTradeData.Symbols)
        {
            bool success = reqSubscribeTradeData.IsCancel ?
                           m_QuoteSubscriptionService.CancelSubscribeTrade(clientUserInfo.UserId, symbol) :
                           m_QuoteSubscriptionService.SubscribeTrade(clientUserInfo.UserId, symbol);
            if (!success)
            {
                sb.Append($"{symbol}");
            }
        }

        if (sb.Length > 0)
        {
            sb.Insert(0, $"请求错误:{(reqSubscribeTradeData.IsCancel ? "订阅Trade数据" : "取消订阅Trade数据")}失败,交易对列表:\n");
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = sb.ToString(),
                Type = NoticeType.Toast,
            });
        }
    }

    [MessageHandler(Request.RequestTypeOneofCase.ReqSubscribeCandlestickUpdate)]
    public void ReqSubscribeCandlestickUpdate(ClientUserInfo clientUserInfo, Request request)
    {
        ReqSubscribeCandlestickUpdate reqSubscribeCandlestickUpdate = request.ReqSubscribeCandlestickUpdate;
        if (!Enum.TryParse(reqSubscribeCandlestickUpdate.BarSize, ignoreCase: false, out BarSize barSize))
        {
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = $"订阅k线更新失败:无法识别的barSize参数:{reqSubscribeCandlestickUpdate.BarSize}",
                Type = NoticeType.Toast,
            });
        }

        if (reqSubscribeCandlestickUpdate.Symbols.Count == 0)
        {
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = $"订阅K线数据失败:symbol列表为空",
                Type = NoticeType.Toast,
            });
        }

        StringBuilder sb = new StringBuilder();
        foreach (var symbol in reqSubscribeCandlestickUpdate.Symbols)
        {
            bool success = reqSubscribeCandlestickUpdate.IsCancel ?
                           m_QuoteSubscriptionService.CancelSubscribeCandleUpdate(clientUserInfo.UserId, symbol, barSize) :
                           m_QuoteSubscriptionService.SubscribeCandleUpdate(clientUserInfo.UserId, symbol, barSize);
            if (!success)
            {
                sb.Append($"{symbol}");
            }
        }

        if (sb.Length > 0)
        {
            sb.Insert(0, $"请求错误:{(reqSubscribeCandlestickUpdate.IsCancel ? "订阅K线更新" : "取消订阅K线数据")}失败,交易对列表:\n");
            m_WebSocketService.PushMessge(clientUserInfo.UserId, new ResNotice()
            {
                Content = sb.ToString(),
                Type = NoticeType.Toast,
            });
        }
    }

    [MessageHandler(Request.RequestTypeOneofCase.ReqTradeRule)]
    public void ReqTradeRule(ClientUserInfo clientUserInfo, Request request)
    {
        ReqTradeRule reqTradeRule = request.ReqTradeRule;
        IEnumerable<SymbolTradeRule> tradeRuleList;
        if (reqTradeRule.SymbolList.Count <= 0) // 查询全体数据
        {
            tradeRuleList = m_QuoteProviderService.QueryAllTradeRule();
        }
        else // 查询指定symbol列表数据
        {
            tradeRuleList = m_QuoteProviderService.QueryTradeRuleBySymbolList(reqTradeRule.SymbolList);
        }
        QuotePushUtil.PushTradeRuleBeanList(m_WebSocketService, clientUserInfo.UserId, tradeRuleList);
    }

    [MessageHandler(Request.RequestTypeOneofCase.ReqSelfSelectedSymbol)]
    public void ReqSelfSelectedSymbol(ClientUserInfo clientUserInfo, Request request)
    {
        ReqSelfSelectedSymbol reqSelfSelectedSymbol = request.ReqSelfSelectedSymbol;
        List<SelfSelectedSymbolGroupData> dataList = m_SelfSelectSymbolService.QuerySymbolGroupData(clientUserInfo.UserId);
        ResSelfSelectedSymbol resSelfSelectedSymbol = new ResSelfSelectedSymbol();

        foreach (SelfSelectedSymbolGroupData data in dataList)
        {
            SelfSelectedSymbolGroupBean bean = ToSelfSelectedSymbolGroupBean(data);
            resSelfSelectedSymbol.GroupList.Add(bean);
        }

        m_WebSocketService.PushMessge(clientUserInfo.UserId, resSelfSelectedSymbol);
    }

    private SelfSelectedSymbolGroupBean ToSelfSelectedSymbolGroupBean(SelfSelectedSymbolGroupData data)
    {
        SelfSelectedSymbolGroupBean bean = new SelfSelectedSymbolGroupBean();
        bean.CanDelete = data.IsDynamicGroup;
        bean.Name = data.Name;
        foreach (var symbolData in data.SymbolList)
        {
            bean.SymbolList.Add(new SelfSelectedSymbolInfoBean() 
            {
                Symbol = symbolData.Symbol,
                Timestamp = symbolData.Timestamp,
                InitialPrice = Convert.ToDouble(symbolData.InitialPrice),
            });
        }

        return bean;
    }
}