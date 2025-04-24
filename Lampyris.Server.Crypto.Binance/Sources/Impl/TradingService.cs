using Binance.Net.Clients;
using Binance.Net.Objects.Models.Futures;
using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using OrderStatus = Lampyris.Crypto.Protocol.Trading.OrderStatus;
using PositionSide = Lampyris.Crypto.Protocol.Trading.PositionSide;

namespace Lampyris.Server.Crypto.Binance;

[Component]
public class TradingService : AbstractTradingService
{
    [Autowired]
    private AccountManager m_AccountManager;

    public override void CancelOrderImpl(int clientUserId, string symbol, int orderId)
    {
        var webSocketClient = m_AccountManager.GetWebSocketClient(clientUserId);
        if (webSocketClient == null)
        {
            throw new InvalidOperationException($"WebSocket client not found for clientUserId: {clientUserId}");
        }

        var result = webSocketClient.UsdFuturesApi.Trading.CancelOrderAsync(
            symbol: symbol,
            orderId: orderId,
            origClientOrderId: null,
            receiveWindow: 5000,
            ct: CancellationToken.None
        ).Result;

        if (result == null || result.Success)
        {
            throw new InvalidOperationException($"Failed to modify order with ID: {orderId}. Error: {result.Error?.Message}");
        }
    }


    /// <summary>
    /// 更新每个子账号的所有持仓信息，并汇总
    /// </summary>
    public void UpdateAllPositionInfo()
    {
        m_AppTradingData.PositionInfoMap.Clear();

        // 对于每一个子账户
        foreach (var pair in m_SubAccountTradingDataMap)
        {
            int subaccountId = pair.Key;

            // 获取 REST 客户端对象
            var restClient = m_AccountManager.GetRestClient(subaccountId);

            // 请求所有持仓信息
            var apiPositionInfoReqResult = restClient.UsdFuturesApi.Trading.GetPositionsAsync().Result;

            if(apiPositionInfoReqResult != null && apiPositionInfoReqResult.Success)
            {
                var apiPositionInfoList = apiPositionInfoReqResult.Data;
                foreach(var apiPositionInfo in apiPositionInfoList)
                {
                    string symbol = apiPositionInfo.Symbol;
                    if(!m_AppTradingData.PositionInfoMap.ContainsKey(symbol))
                    {
                        m_AppTradingData.PositionInfoMap[symbol] = new PositionInfo();
                    }
                    m_AppTradingData.PositionInfoMap[symbol].ApiPositionInfoList.Add(Converter.ToPositionInfo(apiPositionInfo));
                }
            }
        }

        // 汇总
        foreach(var pair in m_AppTradingData.PositionInfoMap)
        {
            PositionInfo systemPositionInfo = pair.Value;

            // 持仓的基本信息取第0个子账户的持仓信息就行了，因为这些信息都是相同的(系统仅支持单向持仓，双向持仓不作考虑)
            // 这里不必判断ApiPositionInfoList是否非空，因为一定存在一个子账户的持仓信息，
            // 不然m_AppTradingData.PositionInfoMap就不会存在这个Symbol对应的PositionInfo了
            var first = systemPositionInfo.ApiPositionInfoList[0];
            systemPositionInfo.Symbol = first.Symbol;
            systemPositionInfo.UpdateTime = first.UpdateTime;
            systemPositionInfo.PositionSide = first.PositionSide;
            systemPositionInfo.MarkPrice = first.MarkPrice;

            foreach (var apiPositionInfo in systemPositionInfo.ApiPositionInfoList)
            {
                systemPositionInfo.InitialMargin += apiPositionInfo.InitialMargin;
                systemPositionInfo.MaintenanceMargin += apiPositionInfo.MaintenanceMargin;
                systemPositionInfo.CostPrice = ((systemPositionInfo.CostPrice * systemPositionInfo.Quantity) + 
                                               (apiPositionInfo.CostPrice + apiPositionInfo.Quantity)) / 
                                               (systemPositionInfo.Quantity + apiPositionInfo.Quantity);
                systemPositionInfo.Quantity += apiPositionInfo.Quantity;
                systemPositionInfo.UnrealizedPnL += apiPositionInfo.UnrealizedPnL;
                
                if (systemPositionInfo.LiquidationPrice == 0)
                {
                    systemPositionInfo.LiquidationPrice = apiPositionInfo.LiquidationPrice;
                }
                else
                {
                    if(systemPositionInfo.PositionSide == PositionSide.Long) // 多头仓位，强平价取各个子账户的最小值
                    {
                        systemPositionInfo.LiquidationPrice = Math.Min(systemPositionInfo.LiquidationPrice,apiPositionInfo.LiquidationPrice);
                    }
                    else
                    {
                        systemPositionInfo.LiquidationPrice = Math.Max(systemPositionInfo.LiquidationPrice, apiPositionInfo.LiquidationPrice);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新每个子账号的所有订单信息，并汇总
    /// </summary>
    public void UpdateAllOpenOrders()
    {
        // 重设数据
        foreach(var pair in m_AppTradingData.OpenOrderInfoList)
        {
            var tradingData = pair.Value;
            foreach (var pair2 in tradingData)
            {
                pair2.Value.Reset();
                pair2.Value.Status = OrderStatus.Filled;
            }
        }

        // 对于每一个子账户
        foreach (var pair in m_SubAccountTradingDataMap) 
        { 
            int subaccountId = pair.Key;
            var tradingData = pair.Value;

            // 获取 REST 客户端对象
            var restClient = m_AccountManager.GetRestClient(subaccountId);

            // 对于子账户中每一个symbol的Order列表
            foreach (var pair2 in tradingData.OpenOrderInfoList)
            {
                // 注意：仅仅需要获取数据库中记录的symbol数据，因为symbol如果不在数据库中，
                // 说明并非通过系统进行下单，系统无需对其进行管理
                var symbol = pair2.Key; 
                var apiOrderStatusInfoMap = pair2.Value;

                if (!m_AppTradingData.OpenOrderInfoList.ContainsKey(symbol))
                {
                    continue; // 非系统订单跳过
                }

                Dictionary<long,OrderStatusInfo> systemOrderStatusInfoMap = m_AppTradingData.OpenOrderInfoList[symbol];

                // 获取该symbol的所有订单
                // PS:如果订单满足如下条件，不会被查询到:
                // 订单的最终状态为 CANCELED 或者 EXPIRED 并且 订单没有任何的成交记录 并且 订单生成时间 +3天<当前时间
                // 订单创建时间 +90天<当前时间
                var result = restClient.UsdFuturesApi.Trading.GetOrdersAsync(symbol, receiveWindow:5000,ct: CancellationToken.None).Result;
                foreach (BinanceUsdFuturesOrder binanceOrderStatusInfo in result.Data)
                {
                    OrderStatusInfo? apiOrderStatusInfo = null;
                    if (apiOrderStatusInfoMap.ContainsKey(binanceOrderStatusInfo.Id))
                    {
                        apiOrderStatusInfo = apiOrderStatusInfoMap[binanceOrderStatusInfo.Id];
                    }
                    else // 非系统下单，无视
                    {
                        continue;
                    }
                    apiOrderStatusInfo = Converter.ToOrderStatusInfo(binanceOrderStatusInfo, apiOrderStatusInfo);

                    // 找到对应的系统订单ID
                    foreach (var pair3 in systemOrderStatusInfoMap)
                    {
                        int systemOrderIndex = pair3.Value.ApiOrderIds.IndexOf(apiOrderStatusInfo.OrderId);
                        if (systemOrderIndex >= 0) // 如果找到了，则汇总该子账户的OrderStatusInfo
                        {
                            OrderStatusInfo systemOrderStatus = systemOrderStatusInfoMap[systemOrderIndex];

                            // count表示当前已经汇总的api订单数目

                            int count = systemOrderStatus.ApiOrderStatusInfoList.Count;
                            // OrderInfo不需要更新，只更新成交数量和平均成本
                            systemOrderStatus.FilledQuantity += apiOrderStatusInfo.FilledQuantity;
                            systemOrderStatus.AvgFilledPrice = (systemOrderStatus.AvgFilledPrice * systemOrderStatus.FilledQuantity + 
                                                                apiOrderStatusInfo.AvgFilledPrice + systemOrderStatus.FilledQuantity) /
                                                               (count + 1);

                            // api订单状态加入到系统状态中
                            systemOrderStatus.ApiOrderStatusInfoList.Add(apiOrderStatusInfo);
                        }
                    }
                }
            }
        }

        // 更新系统订单状态
        // 系统订单的状态由以下因素决定:
        // 1. 子账号订单如果全为同一个状态X，系统订单状态也为X
        // 2. 子账号订单如果有一个状态为OrderStatus.PartiallyFilled,系统订单状态也为OrderStatus.PartiallyFilled
        foreach (var pair in m_AppTradingData.OpenOrderInfoList)
        {
            var tradingData = pair.Value;
            foreach (var pair2 in tradingData)
            {
                var systemOrderStatusInfo = pair2.Value;
                if (systemOrderStatusInfo.Status == OrderStatus.PartiallyFilled)
                {
                    continue;
                }
                foreach(var apiOrderStatusInfo in systemOrderStatusInfo.ApiOrderStatusInfoList)
                {
                    if(apiOrderStatusInfo.Status != systemOrderStatusInfo.Status && systemOrderStatusInfo.Status != OrderStatus.New)
                    {
                        systemOrderStatusInfo.Status = apiOrderStatusInfo.Status;
                    }
                    else if(apiOrderStatusInfo.Status == OrderStatus.PartiallyFilled)
                    {
                        systemOrderStatusInfo.Status = OrderStatus.PartiallyFilled;
                        break;
                    }
                }
            }
        }
    }

    public override void ModifyOrderImpl(int clientUserId, int orderId, OrderInfo updatedOrderInfo)
    {
        // 获取 WebSocket 客户端
        var webSocketClient = m_AccountManager.GetWebSocketClient(clientUserId);
        if (webSocketClient == null)
        {
            throw new InvalidOperationException($"WebSocket client not found for clientUserId: {clientUserId}");
        }

        // 调用 EditOrderAsync 方法
        var result = webSocketClient.UsdFuturesApi.Trading.EditOrderAsync(
            symbol: updatedOrderInfo.Symbol,               // 交易对，例如 ETHUSDT
            side: Converter.ConvertOrderSide(updatedOrderInfo.Side),     // 订单方向
            quantity: updatedOrderInfo.Quantity,          // 修改后的订单数量
            price: updatedOrderInfo.Price,                // 修改后的订单价格（限价单需要）
            priceMatch: null,                              // PriceMatch 参数（根据需求设置）
            orderId: orderId,                              // 订单ID
            origClientOrderId: null,                       // 原始客户端订单ID（如果需要）
            receiveWindow: 5000,                           // 接收窗口时间（可根据需求调整）
            ct: CancellationToken.None                    // 取消令牌
        ).Result;

        // 检查结果
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to modify order with ID: {orderId}. Error: {result.Error?.Message}");
        }

        Console.WriteLine($"Order {orderId} modified successfully.");
    }

    public override long PlaceOrderImpl(int clientUserId, int subAccountId, OrderInfo order)
    {
        // 获取 WebSocket 客户端
        var webSocketClient = m_AccountManager.GetWebSocketClient(clientUserId);
        if (webSocketClient == null)
        {
            throw new InvalidOperationException($"WebSocket client not found for clientUserId: {clientUserId}");
        }

        // 调用 PlaceOrderAsync 方法
        var placeOrderTask = webSocketClient.UsdFuturesApi.Trading.PlaceOrderAsync(
            symbol: order.Symbol,                     // 交易对，例如 BTCUSDT
            side: Converter.ConvertOrderSide(order.Side),           // 订单方向，映射到 API 的枚举值
            type: Converter.ConvertOrderType(order.OrderType),      // 订单类型，映射到 API 的枚举值
            quantity: order.Quantity,                 // 订单数量（以标的为单位）
            price: order.Price,                       // 订单价格（限价单需要）
            timeInForce: Converter.ConvertTimeInForce(order.TifType), // 订单有效方式
            reduceOnly: order.ReduceOnly,             // 是否只减仓
            stopPrice: null,                          // 停止价格
            activationPrice: null,                    // 激活价格
            callbackRate: null                        // 回调比例
        );

        var result = placeOrderTask.Result;
        if (result == null || !result.Success)
        {
            throw new InvalidOperationException("Failed to place order.");
        }

        return result.Data.Result.Id;
    }

    private BinanceRestClient m_RestClient = new BinanceRestClient();

    protected override void UpdateAllSymbolTradeRuleImpl()
    {
        var result = m_RestClient.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync().Result;
        if(result == null || !result.Success)
        {
            return;
        }
        var symbols = result.Data.Symbols;
        foreach (var symbol in symbols)
        {
            // 初始化 SymbolTradeRule 对象
            var tradeRule = m_SymbolTradeRuleInfoMap.ContainsKey(symbol.Name) ? m_SymbolTradeRuleInfoMap[symbol.Name] : new SymbolTradeRule
            {
                Symbol = symbol.Name, 
                MinPrice = symbol.PriceFilter?.MinPrice ?? 0, // 最小价格
                MaxPrice = symbol.PriceFilter?.MaxPrice ?? 0, // 最大价格
                PriceStep = symbol.PriceFilter?.TickSize ?? 0, // 价格步进
                MinQuantity = symbol.LotSizeFilter?.MinQuantity ?? 0, // 最小数量
                MaxQuantity = symbol.LotSizeFilter?.MaxQuantity ?? 0, // 最大数量
                QuantityStep = symbol.LotSizeFilter?.StepSize ?? 0, // 数量步进
                MinNotional = symbol.MinNotionalFilter?.MinNotional ?? 0 // 最小名义价值
            };
        }
    }

    /// <summary>
    /// 获取并汇总指定 symbol 和订单 ID 的交易记录
    /// </summary>
    protected override OrderTradeSummaryData GetAndSummarizeTradesImpl(int accountId, string symbol, long orderId)
    {
        BinanceRestClient client = m_AccountManager.GetRestClient(accountId);

        const int limit = 500; // 每次请求的最大记录数
        var allTrades = new List<BinanceFuturesUsdtTrade>();
        long lastId = 0; // 初始值为 0，表示从最早的记录开始

        while (true)
        {
            // 分批请求交易记录
            var tradesResult = client.UsdFuturesApi.Trading.GetUserTradesAsync(symbol, orderId: orderId, fromId: lastId, limit: limit).Result;

            if (!tradesResult.Success)
            {
                Console.WriteLine($"Failed to fetch trades: {tradesResult.Error}");
                return null;
            }

            var trades = tradesResult.Data;
            int count = trades.Count();

            // 没有更多数据则退出循环
            if (trades == null || count <= 0)
            {
                break;
            }

            // 筛选目标订单的交易记录
            var orderTrades = trades.Where(trade => trade.OrderId == orderId).ToList();

            // 添加到总交易记录列表
            allTrades.AddRange(orderTrades);

            // 更新 lastId 为最后一条记录的 ID
            lastId = trades.Last().Id + 1;

            // 如果返回的记录数少于 limit，说明已经是最后一页
            if (count < limit)
            {
                break;
            }
        }

        // 汇总交易记录
        return SummarizeTrades(allTrades, symbol, orderId);
    }

    /// <summary>
    /// 汇总交易记录
    /// </summary>
    private OrderTradeSummaryData SummarizeTrades(List<BinanceFuturesUsdtTrade> trades, string symbol, long orderId)
    {
        return new OrderTradeSummaryData
        {
            Symbol = symbol,
            OrderId = orderId,
            TotalRealizedPnL = trades.Sum(t => t.RealizedPnl),
            TotalFee = trades.Sum(t => t.Fee),
            TotalQuantity = trades.Sum(t => t.Quantity),
            TradeCount = trades.Count
        };
    }

    protected override void ClosePositionImpl(int accountId, string symbol)
    {
        throw new NotImplementedException();
    }

    protected override LeverageSettingResult SetLeverageImpl(int accountId, string symbol, int leverage)
    {
        throw new NotImplementedException();
    }

    protected override void UpdateAllLeverageBracketImpl()
    {
        throw new NotImplementedException();
    }
}
