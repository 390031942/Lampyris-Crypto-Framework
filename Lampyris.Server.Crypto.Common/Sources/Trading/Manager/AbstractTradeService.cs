namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;
using Lampyris.CSharp.Common;

[Component]
public abstract class AbstractTradeService
{
    public class PerUserTradeData
    {
        // 当前持仓信息
        public List<PositionInfo> PositionInfoList = new List<PositionInfo>();

        // 当前订单信息
        public List<OrderStatusInfo> ActiveOrderInfoList = new List<OrderStatusInfo>();

        // 历史订单信息
        public List<OrderStatusInfo> HistoricalOrderInfoList = new List<OrderStatusInfo>();

        // 杠杆设置信息
        public Dictionary<string, int> LeverageSettingsDataMap = new Dictionary<string, int>();

        // 杠杆分层信息
        public Dictionary<string, LeverageBracketInfo> LeverageBracketDataMap = new Dictionary<string, LeverageBracketInfo>();
    }

    /// <summary>
    /// 创建订单(实现)
    /// </summary>
    /// <param name="clientUserId">用户ID</param>
    /// <param name="order">订单信息</param>
    public abstract void PlaceOrderImpl(int clientUserId, OrderInfo order);

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="clientUserId">用户ID</param>
    /// <param name="order"></param>
    public void PlaceOrder(int clientUserId, OrderInfo order)
    {
        PlaceOrderImpl(clientUserId, order);
        Console.WriteLine($"Order placed by user {clientUserId}: {order})");
    }

    /// <summary>
    /// 修改订单(实现)
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="updatedOrderInfo">待修改的订单信息</param>
    public abstract void ModifyOrderImpl(int clientUserId, int orderId, OrderInfo updatedOrderInfo);

    /// <summary>
    /// 修改订单
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="updatedOrderInfo">待修改的订单信息</param>
    public void ModifyOrder(int orderId, OrderInfo updatedOrder)
    {
        Console.WriteLine($"Order modified: {orderId}");
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    /// <param name="orderId">订单ID</param>
    public void CancelOrder(int orderId)
    {
        Console.WriteLine($"Order canceled: {orderId}");
    }

    /// <summary>
    /// 取消订单(实现)
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="updatedOrderInfo">待修改的订单信息</param>
    public abstract void CancelOrderAsync(int orderId);

    /// <summary>
    /// 清仓指定symbol
    /// </summary>
    /// <param name="symbol">交易对</param>
    public void ClosePosition(int clientUserId, string symbol)
    {
        OrderInfo orderInfo = new OrderInfo();
        Console.WriteLine($"Position closed for symbol: {symbol}, Order ID: {order.OrderId}");
    }

    /// <summary>
    /// 清仓全部symbol
    /// </summary>
    /// <param name="symbol">交易对</param>
    public void CloseAllPosition(string symbol)
    {
        OrderInfo orderInfo = new OrderInfo();
        Console.WriteLine($"Position closed for symbol: {symbol}, Order ID: {order.OrderId}");
    }

    /// <summary>
    /// 批量一键清仓()
    /// </summary>
    /// <param name="symbols"></param>
    public void CloseAllPositions(List<string> symbols)
    {
        foreach (var symbol in symbols)
        {
            var ordersToClose = _activeOrders.Where(o => o.Symbol == symbol).ToList();
            foreach (var order in ordersToClose)
            {
                _activeOrders.Remove(order);
                Console.WriteLine($"Position closed for symbol: {symbol}, Order ID: {order.OrderId}");
            }
        }
    }

    // 查询当前活动订单
    public List<OrderInfo> QueryActiveOrders(string symbol = null)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            return _activeOrders;
        }

        return _activeOrders.Where(o => o.Symbol == symbol).ToList();
    }

    // 查询历史订单
    public List<OrderStatusInfo> QueryHistoricalOrders(string symbol = null, long? beginTime = null, long? endTime = null)
    {
        return _historicalOrders.Where(o =>
            (string.IsNullOrEmpty(symbol) || o.OrderBean.Symbol == symbol) &&
            (!beginTime.HasValue || o.OrderBean.CreatedTime >= beginTime.Value) &&
            (!endTime.HasValue || o.OrderBean.CreatedTime <= endTime.Value)).ToList();
    }

    // 查询杠杆倍数
    public List<LeverageBean> QueryLeverage(string symbol = null)
    {
        var result = new List<LeverageBean>();

        if (string.IsNullOrEmpty(symbol))
        {
            foreach (var kvp in _leverageSettings)
            {
                result.Add(new LeverageBean { Symbol = kvp.Key, Leverage = kvp.Value });
            }
        }
        else if (_leverageSettings.ContainsKey(symbol))
        {
            result.Add(new LeverageBean { Symbol = symbol, Leverage = _leverageSettings[symbol] });
        }

        return result;
    }

    // 设置杠杆倍数
    public void SetLeverage(List<LeverageBean> leverageBeans)
    {
        foreach (var bean in leverageBeans)
        {
            _leverageSettings[bean.Symbol] = bean.Leverage;
            Console.WriteLine($"Leverage set for {bean.Symbol}: {bean.Leverage}x");
        }
    }

    // 查询杠杆分层
    public List<SymbolLeverageBracketBean> QueryLeverageBracket(string symbol = null)
    {
        var result = new List<SymbolLeverageBracketBean>();

        if (string.IsNullOrEmpty(symbol))
        {
            foreach (var kvp in _leverageBrackets)
            {
                result.Add(new SymbolLeverageBracketBean { BeanList = kvp.Value });
            }
        }
        else if (_leverageBrackets.ContainsKey(symbol))
        {
            result.Add(new SymbolLeverageBracketBean { BeanList = _leverageBrackets[symbol] });
        }

        return result;
    }
}
