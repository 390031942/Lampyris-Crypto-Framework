namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;
using Lampyris.CSharp.Common;

[Component]
public abstract class AbstractTradeService
{
    /// <summary>
    /// 创建订单(实现)
    /// </summary>
    /// <param name="order">订单信息</param>
    public abstract void PlaceOrderImpl(Order order);

    // 下单
    public void PlaceOrder(int clientUserId, OrderInfo order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        // 模拟生成订单ID
        order.OrderId = Guid.NewGuid().ToString();
        _activeOrders.Add(order);

        Console.WriteLine($"Order placed by user {clientUserId}: {order})");
    }

    // 修改订单
    public void ModifyOrder(string orderId, OrderInfo updatedOrder)
    {
        var existingOrder = _activeOrders.FirstOrDefault(o => o.OrderId == orderId);
        if (existingOrder == null)
        {
            throw new Exception($"Order with ID {orderId} not found.");
        }

        // 更新订单信息
        existingOrder.Symbol = updatedOrder.Symbol;
        existingOrder.Quantity = updatedOrder.Quantity;
        existingOrder.Price = updatedOrder.Price;
        existingOrder.OrderType = updatedOrder.OrderType;

        Console.WriteLine($"Order modified: {orderId}");
    }

    // 撤单
    public void CancelOrder(string orderId)
    {
        var order = _activeOrders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new Exception($"Order with ID {orderId} not found.");
        }

        _activeOrders.Remove(order);
        Console.WriteLine($"Order canceled: {orderId}");
    }

    // 一键清仓
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
