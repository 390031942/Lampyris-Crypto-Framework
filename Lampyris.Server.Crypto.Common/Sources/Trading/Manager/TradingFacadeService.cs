using CryptoExchange.Net.CommonObjects;
using Lampyris.Crypto.Protocol.App;
using Lampyris.Crypto.Protocol.Common;
using Lampyris.Crypto.Protocol.Quote;
using Lampyris.Crypto.Protocol.Trading;
using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component]
public class TradingFacadeService
{
    [Autowired]
    private WebSocketService m_WebSocketService;

    [Autowired]
    private AbstractQuoteProviderService m_AbstractQuoteProviderService;

    [Autowired]
    private AbstractTradingService m_AbstractTradingService;

    /// <summary>
    /// 下单请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqPlaceOrder)]
    public void ReqPlaceOrder(ClientUserInfo clientUserInfo, Request request)
    {
        ReqPlaceOrder reqPlaceOrder = request.ReqPlaceOrder;
        OrderBean bean = reqPlaceOrder.OrderBean;

        OrderInfo orderInfo = OrderInfo.ValueOf(bean);
        orderInfo.OrderId = UniqueIdGenerator.Get();
        orderInfo.ClientUserId = clientUserInfo.UserId;

        m_AbstractTradingService.PlaceOrder(clientUserInfo.UserId, orderInfo);
    }

    /// <summary>
    /// 修改订单请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqModifyOrder)]
    public void ReqModifyOrder(ClientUserInfo clientUserInfo, Request request)
    {
        ReqModifyOrder reqModifyOrder = request.ReqModifyOrder;
        OrderBean bean = reqModifyOrder.OrderBean;

        OrderInfo orderInfo = OrderInfo.ValueOf(bean);
        orderInfo.OrderId = reqModifyOrder.OrderId;
        orderInfo.ClientUserId = clientUserInfo.UserId;

        m_AbstractTradingService.ModifyOrder(clientUserInfo.UserId, orderInfo);
    }

    /// <summary>
    /// 撤单请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqCancelOrder)]
    public void ReqCancelOrder(ClientUserInfo clientUserInfo, Request request)
    {
        ReqCancelOrder reqCancelOrder = request.ReqCancelOrder;
        m_AbstractTradingService.CancelOrder(reqCancelOrder.OrderId);
    }

    /// <summary>
    /// 处理交易规则请求
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqTradeRule)]
    public void ReqTradeRule(ClientUserInfo clientUserInfo, Request request)
    {
        ReqTradeRule reqTradeRule = request.ReqTradeRule;
        ResTradeRule resTradeRule = new ResTradeRule();

        if (reqTradeRule.SymbolList.Count <= 0) 
        {
            foreach(SymbolTradeRule tradeRule in m_AbstractTradingService.GetAllSymbolTradeRule())
            {
                resTradeRule.BeanList.Add(tradeRule.ToBean());
            }
        }
        else
        {
            foreach (var symbol in reqTradeRule.SymbolList)
            {
                resTradeRule.BeanList.Add(m_AbstractTradingService.GetSymbolTradeRule(symbol).ToBean());
            }
        }
        m_WebSocketService.PushMessge(clientUserInfo.UserId, resTradeRule);
    }

    /// <summary>
    /// 一键清仓请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqOneKeyClosePosition)]
    public void ReqOneKeyClosePosition(ClientUserInfo clientUserInfo, Request request)
    {
        ReqOneKeyClosePosition reqOneKeyClosePosition = request.ReqOneKeyClosePosition;
        m_AbstractTradingService.ClosePositions(reqOneKeyClosePosition.Symbols.ToList());
    }

    /// <summary>
    /// 查询活动订单请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqQueryActiveOrders)]
    public void ReqQueryActiveOrders(ClientUserInfo clientUserInfo, Request request)
    {
        ReqQueryActiveOrders reqQueryActiveOrders = request.ReqQueryActiveOrders;
        List<OrderStatusInfo> orderStatusInfoList = m_AbstractTradingService.QueryActiveOrders(reqQueryActiveOrders.Symbol);

        ResQueryOrders resQueryOrders = new ResQueryOrders();
        resQueryOrders.IsActive = true;

        foreach (OrderStatusInfo orderStatusInfo in orderStatusInfoList)
        {
            resQueryOrders.BeanList.Add(orderStatusInfo.ToBean());
        }

        m_WebSocketService.PushMessge(clientUserInfo.UserId, resQueryOrders);
    }

    /// <summary>
    /// 查询历史订单请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqQueryHistoricalOrders)]
    public void ReqQueryHistoricalOrders(ClientUserInfo clientUserInfo, Request request)
    {
        ReqQueryHistoricalOrders reqQueryHistoricalOrders = request.ReqQueryHistoricalOrders;
        List<OrderStatusInfo> orderStatusInfoList = m_AbstractTradingService.QueryHistoricalOrders(reqQueryHistoricalOrders.Symbol, 
            reqQueryHistoricalOrders.BeginTime, reqQueryHistoricalOrders.EndTime);

        ResQueryOrders resQueryOrders = new ResQueryOrders();
        resQueryOrders.IsActive = false;

        foreach(OrderStatusInfo orderStatusInfo in orderStatusInfoList)
        {
            resQueryOrders.BeanList.Add(orderStatusInfo.ToBean());
        }
        m_WebSocketService.PushMessge(clientUserInfo.UserId, resQueryOrders);
    }

    /// <summary>
    /// 查询当前持仓请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqQueryPositions)]
    public void ReqQueryPositions(ClientUserInfo clientUserInfo, Request request)
    {
        ReqQueryPositions reqQueryPositions = request.ReqQueryPositions;
        List<PositionInfo> positionInfoList = m_AbstractTradingService.QueryPositions(reqQueryPositions.Symbol);

        ResQueryPositions resQueryPositions = new ResQueryPositions();
        foreach (PositionInfo positionInfo in positionInfoList)
        {
            resQueryPositions.BeanList.Add(positionInfo.ToBean());
        }
        m_WebSocketService.PushMessge(clientUserInfo.UserId, resQueryPositions);
    }

    /// <summary>
    /// 批量设置杠杆倍数请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqSetLeverage)]
    public void ReqSetLeverage(ClientUserInfo clientUserInfo, Request request)
    {
        ReqSetLeverage reqSetLeverage = request.ReqSetLeverage;
        foreach(var bean in reqSetLeverage.BeanList)
        {
            m_AbstractTradingService.SetLeverage(bean.Symbol, bean.Leverage);
        }
    }

    /// <summary>
    /// 查询杠杆倍数请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqQueryLeverage)]
    public void ReqQueryLeverage(ClientUserInfo clientUserInfo, Request request)
    {
        ReqQueryLeverage reqQueryLeverage = request.ReqQueryLeverage;
        ResQueryLeverage resQueryLeverage = new ResQueryLeverage();

        if (!string.IsNullOrEmpty(reqQueryLeverage.Symbol))
        {
            var leverageInfo = m_AbstractTradingService.QueryLeverage(reqQueryLeverage.Symbol);
            if (leverageInfo != null)
            {
                resQueryLeverage.BeanList.Add(leverageInfo.ToBean());
            }
        }
        else
        {
            foreach(string symbol in m_AbstractQuoteProviderService.GetSymbolList())
            {
                var leverageInfo = m_AbstractTradingService.QueryLeverage(reqQueryLeverage.Symbol);
                if (leverageInfo != null)
                {
                    resQueryLeverage.BeanList.Add(leverageInfo.ToBean());
                }
            }
        }
        m_WebSocketService.PushMessge(clientUserInfo.UserId, resQueryLeverage);
    }

    /// <summary>
    /// 查询杠杆分层请求处理
    /// </summary>
    [MessageHandler(Request.RequestTypeOneofCase.ReqQueryLeverageBracket)]
    public void ReqQueryLeverageBracket(ClientUserInfo clientUserInfo, Request request)
    {
        ReqQueryLeverageBracket reqQueryLeverageBracket = request.ReqQueryLeverageBracket;
        ResQueryLeverageBracket resQueryLeverageBracket = new ResQueryLeverageBracket();
         
        if (!string.IsNullOrEmpty(reqQueryLeverageBracket.Symbol))
        {
            var leverageBracket = m_AbstractTradingService.QueryLeverageBracket(reqQueryLeverageBracket.Symbol);
            var symbolBean = new SymbolLeverageBracketBean();
            foreach (var info in leverageBracket)
            {
                symbolBean.BeanList.Add(info.ToBean());
            }
            resQueryLeverageBracket.BeanList.Add(symbolBean);
        }
        else
        {
            foreach (string symbol in m_AbstractQuoteProviderService.GetSymbolList())
            {
                var leverageBracket = m_AbstractTradingService.QueryLeverageBracket(symbol);
                var symbolBean = new SymbolLeverageBracketBean();
                foreach (var info in leverageBracket)
                {
                    symbolBean.BeanList.Add(info.ToBean());
                }
                resQueryLeverageBracket.BeanList.Add(symbolBean);
            }
        }
    }
}
