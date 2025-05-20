namespace Lampyris.Server.Crypto.Common;

using Lampyris.Crypto.Protocol.Trading;
using Lampyris.CSharp.Common;
using System;

/// <summary>
/// 1. 负责处理下单/修改订单/撤单功能，
/// 注意，这里实际上是 对子账号进行下单/修改订单/撤单，因此需要一个分仓功能
/// PS：分仓功能需要在下单时候计算 每个子账号 可买多头/空头 数量的总价值，
/// 并对每个子账号产生一个实际的订单，并通过具体的API发送给交易所,同理
/// 修改/撤单时候也需要对每个子账号的订单进行操作(下述功能都涉及分仓原理，不再赘叙)
/// 
/// 2.修改symbol对应的杠杆值
/// 
/// 3.订单查询功能，查询的订单来自于数据库，每个订单需对应 对应若干个子账号的实际订单ID
/// 
/// 4.针对symbol的一键清仓功能
/// 
/// 5.查询持仓功能，持仓数据包括成本价，数量，名义价值，浮动盈亏，强平价等信息
/// 注意,强平价指的是若干个子账号持仓中的最高强平价格(多头持仓)/最低强平价格(空头持仓)
/// 到达强平价格后，由程序程序执行一键清仓
/// </summary>
[Component]
public abstract class AbstractTradingService:ILifecycle
{
    [Autowired]
    protected TradeDBService m_DBService;

    public override int Priority => 4;

    public class SubAccountTradingData
    {
        // 当前持仓信息(key = Symbol)
        public Dictionary<string, PositionInfo> PositionInfoMap = new();

        // 当前订单信息(key = Symbol, value = Dictionary<OrderId,OrderStatusInfo>)
        public Dictionary<string,Dictionary<long,OrderStatusInfo>> OpenOrderInfoList = new();

        // 历史订单信息(key = Symbol, value = 订单状态列表, 按照时间由近到久排序)
        public Dictionary<string,List<OrderStatusInfo>> HistoricalOrderInfoList = new();

        // 杠杆设置信息(key = Symbol, value = 杠杆倍数)
        public Dictionary<string, int> LeverageSettingsDataMap = new Dictionary<string, int>();

        // 杠杆分层信息
        public Dictionary<string, List<LeverageBracketInfo>> LeverageBracketDataMap = new Dictionary<string, List<LeverageBracketInfo>>();
    }

    // 子账户的交易数据
    protected Dictionary<int, SubAccountTradingData> m_SubAccountTradingDataMap = new Dictionary<int, SubAccountTradingData>();

    // 每个symbol在当前杠杆倍数设定下对应的最大可开仓名义价值
    protected Dictionary<string, decimal> m_SymbolMaxNotionalMap = new Dictionary<string, decimal>();

    // 每个symbol的历史仓位记录，仅仅在清仓后被记录
    protected Dictionary<string, List<HistoricalPositionInfo>> m_SymbolHistoricalPositionDataMap = new Dictionary<string, List<HistoricalPositionInfo>>();

    // 系统的交易数据m_AppTradingData，可以认为是各个子账户的汇总,需要被存储于数据库中，其中:
    // 
    // 对于杠杆设置信息，对于某个symbol的杠杆倍数， 为各个子账户中杠杆倍数的最大值，
    // 最大开仓名义价值为 各个子账户中当前杠杆倍数中对应最大开仓名义价值的总和
    // 比如有2个子账户，子账户0对"BTCUSDT"的杠杆倍数设置为10倍，最大开仓名义价值为1000000 USDT
    // 而子账户1对"BTCUSDT"的杠杆倍数设置为、20倍，最大开仓名义价值为500000 USDT
    // 则系统账户认为对"BTCUSDT"的杠杆倍数设置为20倍，最大开仓名义价值为 1000000 + 500000 = 1500000 USDT
    // 
    // 此外，杠杆分层信息的统计也是和上述原理相同
    protected SubAccountTradingData m_AppTradingData = new SubAccountTradingData();

    // 交易对交易规则
    protected Dictionary<string,SymbolTradeRule> m_SymbolTradeRuleInfoMap = new Dictionary<string,SymbolTradeRule>();

    [Autowired]
    protected AbstractAccountManagerBase m_AccountManager;

    protected abstract void APIUpdateSubaccountInfoImpl(SubTradeAccount account);

    public override void OnStart()
    {
        base.OnStart();

        m_AccountManager.ForeachSubAccount((account) =>
        {
            m_SubAccountTradingDataMap[account.AccountId] = new SubAccountTradingData();
        });
    }

    #region 订单创建

    /// <summary>
    /// 创建订单(实现)
    /// </summary>
    /// <param name="clientUserId">用户ID</param>
    /// <param name="subAccountId">子账户ID</param>
    /// <param name="order">订单信息</param>
    /// <returns>订单IDmyz</returns>
    public abstract long PlaceOrderImpl(int clientUserId, int subAccountId, OrderInfo order);

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="clientUserId">用户ID</param>
    /// <param name="order"></param>
    public void PlaceOrder(int clientUserId, OrderInfo order)
    {
        if(order == null)
        {
            return;
        }

        if(!m_SymbolTradeRuleInfoMap.ContainsKey(order.Symbol))
        {
            Logger.LogWarning($"Unabled to place order: {order}, Symbol = \"{order.Symbol}\" is not tradable");
        }

        OrderStatusInfo orderStatusInfo = new OrderStatusInfo();
        orderStatusInfo.OrderId = UniqueIdGenerator.Get();
        orderStatusInfo.Status = OrderStatus.New;

        m_AccountManager.ForeachSubAccount(accountInfo =>
        {
            PlaceOrderImpl(clientUserId, accountInfo.AccountId, order);
        });
        Logger.LogInfo($"Order placed by user {clientUserId}: {order})");

        Task.Run(() =>
        {
            m_DBService.GetTable<OrderInfo>().Insert(order);
            m_DBService.GetTable<OrderStatusInfo>().Insert(orderStatusInfo);
        });
    }
    #endregion

    #region 订单修改
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
    #endregion

    #region 取消订单
    /// <summary>
    /// 取消订单
    /// </summary>
    /// <param name="orderId">订单ID</param>
    public void CancelOrder(long orderId)
    {
        Console.WriteLine($"Order canceled: {orderId}");
    }

    /// <summary>
    /// 取消订单(实现)
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="updatedOrderInfo">待修改的订单信息</param>
    public abstract void CancelOrderImpl(int clientUserId, string symbol, int orderId);
    #endregion

    #region 一键清仓
    /// <summary>
    /// 清仓指定symbol(实现)
    /// </summary>
    /// <param name="symbol">交易对</param>
    protected abstract void APIClosePositionImpl(int accountId, string symbol);
    
    /// <summary>
    /// 批量一键清仓
    /// </summary>
    /// <param name="symbols">交易对列表</param>
    public void ClosePositions(List<string> symbols)
    {
        if(symbols.Count == 0)
        {
            return;
        }

        Task.Run(() =>
        {
            Task[] taskList = new Task[m_SubAccountTradingDataMap.Count];
            foreach (var symbol in symbols)
            {
                int index = 0;
                foreach(var pair in m_SubAccountTradingDataMap)
                {
                    int accountId = pair.Key;
                    taskList[index++] = Task.Run(() => { APIClosePositionImpl(accountId,symbol); });
                }
                Task.WaitAll(taskList);
            }
        });
    }
    #endregion

    #region 查询当前或历史订单
    /// <summary>
    /// 查询当前订单
    /// </summary>
    /// <param name="symbol">交易对(可选)</param>
    /// <returns></returns>
    public List<OrderStatusInfo> QueryActiveOrders(string? symbol = null)
    {
        List<OrderStatusInfo> result = new List<OrderStatusInfo>();
        return result;
    }

    /// <summary>
    /// 查询历史订单
    /// </summary>
    /// <param name="symbol">交易对(可选)</param>
    /// <param name="beginTime">开始时间过滤</param>
    /// <param name="endTime">结束时间过滤</param>
    /// <returns></returns>
    public List<OrderStatusInfo> QueryHistoricalOrders(string? symbol = null, long? beginTime = null, long? endTime = null)
    {
        if(string.IsNullOrEmpty(symbol) ||  m_AppTradingData.HistoricalOrderInfoList.ContainsKey(symbol) || 
            (beginTime.HasValue && endTime.HasValue && beginTime > endTime))
        {
            return null;
        }

        var list = m_AppTradingData.HistoricalOrderInfoList[symbol];
        return list.Where(o =>
            (o.OrderInfo.Symbol == symbol) &&
            (!beginTime.HasValue || o.OrderInfo.CreatedTime >= beginTime.Value) &&
            (!endTime.HasValue || o.OrderInfo.CreatedTime <= endTime.Value)).ToList();
    }
    #endregion

    #region 杠杆相关
    private LeverageSetting MakeLeverageSetting(SubAccountTradingData subAccountTradingData, string symbol, int leverage)
    {
        var value = m_SymbolMaxNotionalMap.TryGetValue(symbol, out decimal maxNotional);
        if(!value)
        {
            return null;
        }

        LeverageSetting leverageSetting = new LeverageSetting();
        leverageSetting.Symbol = symbol;
        leverageSetting.Leverage = leverage;
        leverageSetting.MaxNotional = maxNotional;

        return leverageSetting;
    }

    // 查询杠杆倍数
    public LeverageSetting? QueryLeverage(string symbol)
    {
        m_AppTradingData.LeverageSettingsDataMap.TryGetValue(symbol, out var leverage);
        return MakeLeverageSetting(m_AppTradingData, symbol, leverage);
    }

    public class LeverageSettingResult
    {
        /// <summary>
        /// 交易对
        /// </summary>
        public string Symbol;

        /// <summary>
        /// 杠杆设置错误信息
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// 设置后的杠杆倍数
        /// </summary>
        public int Leverage;

        /// <summary>
        /// 设置后的最大可开仓名义价值
        /// </summary>
        public double MaxNotional;
    }

    /// <summary>
    /// 设置杠杆倍数实现
    /// </summary>
    /// <param name="accountId">子账号ID</param>
    /// <param name="symbol">交易对</param>
    /// <param name="leverage">杠杆倍数值</param>
    protected abstract LeverageSettingResult APISetLeverageImpl(int accountId, string symbol, int leverage);

    /// <summary>
    /// 更新全体symbol的杠杆阶梯
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="leverage"></param>
    /// <returns></returns>
    protected abstract void UpdateAllLeverageBracketImpl();

    // 设置杠杆倍数
    public LeverageSettingResult SetLeverage(string symbol, int leverage)
    {
        Task[] taskList = new Task[m_SubAccountTradingDataMap.Count];
        LeverageSettingResult[] results = new LeverageSettingResult[m_SubAccountTradingDataMap.Count];
        
        int index = 0;
        foreach(var pair in m_SubAccountTradingDataMap)
        {
            int accountId = pair.Key;
            taskList[index] = Task.Run(() => 
            { 
                results[index] = APISetLeverageImpl(accountId, symbol, leverage);
            });
            index++;
        }
        Task.WaitAll(taskList);

        LeverageSettingResult result = new LeverageSettingResult();
        foreach (var subAccountResult in results)
        {
            if (!string.IsNullOrEmpty(subAccountResult.ErrorMessage))
            {
                result.ErrorMessage = result.ErrorMessage + "\n" + subAccountResult.ErrorMessage;
            }
            result.Leverage = Math.Max(result.Leverage, subAccountResult.Leverage);
            result.MaxNotional = result.MaxNotional + subAccountResult.MaxNotional;
        }

        // 杠杆设置存在失败的情况，需要重新获取杠杆阶梯
        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            UpdateAllLeverageBracketImpl();
        }
        return result;
    }   

    /// <summary>
    /// 查询指定交易对杠杆分层
    /// </summary>
    /// <param name="symbol">交易对</param>
    /// <returns></returns>
    public List<LeverageBracketInfo> QueryLeverageBracket(string symbol)
    {
        m_AppTradingData.LeverageBracketDataMap.TryGetValue(symbol, out var result);
        return result;
    }

    private decimal GetMaxNotionalByLeverage(List<LeverageBracketInfo> leverageBracketInfoList, int leverage)
    {
        decimal maxNotional = -1.0m;
        foreach (LeverageBracketInfo leverageBracketInfo in leverageBracketInfoList)
        {
            if (leverage <= leverageBracketInfo.Leverage)
            {
                maxNotional = Math.Max(maxNotional, leverageBracketInfo.NotionalCap);
            }
        }
        return maxNotional;
    }

    /// <summary>
    /// 更新子账户对应的杠杆倍数信息，其只能在收到API消息时 被外部调用，不可以手动调用
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="symbol"></param>
    /// <param name="leverage"></param>
    public void AccountUpdateLeverage(int accountId, string? symbol, int leverage)
    {
        if (!string.IsNullOrEmpty(symbol))
        {
            // 更新子账户的杠杆倍数信息
            {
                var tradingData = m_SubAccountTradingDataMap[accountId];
                if (tradingData != null && tradingData.PositionInfoMap.ContainsKey(symbol))
                {
                    tradingData.LeverageSettingsDataMap[symbol] = leverage;
                }
            }

            // 更新对symbol的杠杆汇总信息,包括杠杆倍数和最大可开仓名义价值
            {
                int appLeverage = -1;
                decimal appMaxNotional = 0.0m;

                foreach (var pair in m_SubAccountTradingDataMap)
                {
                    var tradingData = pair.Value;
                    appLeverage = Math.Max(appLeverage, tradingData.LeverageSettingsDataMap[symbol]);
                    appMaxNotional += GetMaxNotionalByLeverage(tradingData.LeverageBracketDataMap[symbol], leverage);
                }
            }
        }
    }
    #endregion

    #region 外部更新接口
    /// <summary>
    /// 外部调用-更新订单状态，其只能在收到API消息时 被外部调用，不可以手动调用
    /// </summary>
    /// <param name="accountId">子账户ID</param>
    /// <param name="orderStatusInfo">订单状态</param>
    public void UpdateOrderStatus(int accountId, OrderStatusInfo orderStatusInfo)
    {

    }

    /// <summary>
    /// 外部调用-更新子账户的仓位信息，其只能在收到API消息时 被外部调用，不可以手动调用
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="updateInfo"></param>
    public void UpdatePosition(int accountId, PositionUpdateInfo updateInfo)
    {
        if (m_SubAccountTradingDataMap.ContainsKey(accountId))
        {
            var tradingData = m_SubAccountTradingDataMap[accountId];
            if (tradingData != null && tradingData.PositionInfoMap.ContainsKey(updateInfo.Symbol))
            {
                var apiPositionInfo = tradingData.PositionInfoMap[updateInfo.Symbol];
                var systemPositionInfo = m_AppTradingData.PositionInfoMap[updateInfo.Symbol];

                // 增量更新，先减去该子账户的持仓数据
                systemPositionInfo.UnrealizedPnL -= apiPositionInfo.UnrealizedPnL;
                systemPositionInfo.RealizedPnL -= apiPositionInfo.RealizedPnL;
                systemPositionInfo.CostPrice = ((systemPositionInfo.CostPrice * systemPositionInfo.Quantity) -
                                                (apiPositionInfo.CostPrice * apiPositionInfo.Quantity)) /
                                                (systemPositionInfo.Quantity - apiPositionInfo.Quantity);
                systemPositionInfo.Quantity -= apiPositionInfo.Quantity;

                // 更新子账户的数据，再汇总到systemPositionInfo
                apiPositionInfo.Quantity = updateInfo.Quantity;
                apiPositionInfo.UnrealizedPnL = apiPositionInfo.UnrealizedPnL;
                apiPositionInfo.RealizedPnL = apiPositionInfo.RealizedPnL;

                systemPositionInfo.UnrealizedPnL += apiPositionInfo.UnrealizedPnL;
                systemPositionInfo.RealizedPnL += apiPositionInfo.RealizedPnL;
                systemPositionInfo.CostPrice = ((systemPositionInfo.CostPrice * systemPositionInfo.Quantity) +
                                                (apiPositionInfo.CostPrice * apiPositionInfo.Quantity)) /
                                                (systemPositionInfo.Quantity + apiPositionInfo.Quantity);
                systemPositionInfo.Quantity += apiPositionInfo.Quantity;

                // 更新时间
                apiPositionInfo.UpdateTime = updateInfo.UpdateTime;
                systemPositionInfo.UpdateTime = apiPositionInfo.UpdateTime;
            }
        }
    }

    /// <summary>
    /// 外部调用-更新持仓的ADL自动减仓优先级
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="symbol"></param>
    /// <param name="v"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void UpdateADLLevel(int accountId, string symbol, int adl)
    {
        if (m_SubAccountTradingDataMap.ContainsKey(accountId))
        {
            var tradingData = m_SubAccountTradingDataMap[accountId];
            if (tradingData != null)
            {
                var positionInfo = tradingData.PositionInfoMap.GetValueOrDefault(symbol);
                if (positionInfo == null)
                {
                    positionInfo = new PositionInfo();
                    tradingData.PositionInfoMap[symbol] = positionInfo;
                }

                positionInfo.AutoDeleveragingLevel = adl;
            }
        }
    }

    /// <summary>
    /// 外部调用，设置清仓的交易对，记录"历史仓位"信息
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="symbol"></param>
    public void SetClearedPositionForSymbol(int accountId, string symbol)
    {
        if (m_SymbolHistoricalPositionDataMap.ContainsKey(symbol))
        {
            m_SymbolHistoricalPositionDataMap[symbol] = new List<HistoricalPositionInfo>();
        }

        // 找到上一个的历史持仓的信息，获取最后一个订单ID(若不存在则为0)
        // 然后根据这个订单ID来筛选得到持仓期间的所有订单ID
        var historicalPositionList = m_SymbolHistoricalPositionDataMap[symbol];
        long startOrderId = historicalPositionList.Count > 0 ? (historicalPositionList.Last().CorrespondingOrderIdList.Last()) : 0L;

        if (m_AppTradingData.HistoricalOrderInfoList.ContainsKey(symbol))
        {
            // 找到大于startOrderId且有成交的订单列表，统计并得到HistoricalPositionInfo
            List<OrderStatusInfo> statusInfo = m_AppTradingData.HistoricalOrderInfoList[symbol].Where(o =>
            o.OrderId > startOrderId && o.FilledQuantity > 0).ToList();

            HistoricalPositionInfo historicalPositionInfo = new HistoricalPositionInfo();

            int index = 0;
            foreach (var orderStatusInfo in statusInfo)
            {
                if (index == 0)
                {
                    historicalPositionInfo.Symbol = orderStatusInfo.OrderInfo.Symbol;
                    historicalPositionInfo.PositionSide = orderStatusInfo.OrderInfo.PositionSide;
                }
                OrderTradeSummaryData summaryData = GetAndSummarizeTradesImpl(accountId, symbol, orderStatusInfo.OrderId);
                historicalPositionInfo.RealizedPnL = summaryData.TotalRealizedPnL;
                historicalPositionInfo.Fee = summaryData.TotalFee;

                historicalPositionInfo.CorrespondingOrderIdList.Add(orderStatusInfo.OrderId);
            }
            historicalPositionList.Add(historicalPositionInfo);
        }
    }

    #endregion

    #region 交易规则
    /// <summary>
    /// 更新全体交易对的交易规则
    /// </summary>
    protected abstract void UpdateAllSymbolTradeRuleImpl();

    /// <summary>
    /// 获取全体交易对的交易规则
    /// </summary>
    public List<SymbolTradeRule> GetAllSymbolTradeRule()
    {
        return m_SymbolTradeRuleInfoMap.Values.ToList();
    }

    /// <summary>
    /// 获取指定交易对的交易规则
    /// </summary>
    public SymbolTradeRule GetSymbolTradeRule(string symbol)
    {
        if(m_SymbolTradeRuleInfoMap.ContainsKey(symbol))
        {
            return m_SymbolTradeRuleInfoMap[symbol];
        }

        return null;
    }
    #endregion

    #region 查询持仓
    /// <summary>
    /// 查询持仓信息列表
    /// </summary>
    /// <param name="symbol">指定交易对，如果为空则返回全部持仓</param>
    /// <returns></returns>
    public List<PositionInfo> QueryPositions(string? symbol)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            return m_AppTradingData.PositionInfoMap.Values.ToList();
        }
        else
        {
            return m_AppTradingData.PositionInfoMap.ContainsKey(symbol) ? 
                new List<PositionInfo>() { m_AppTradingData.PositionInfoMap[symbol] } : null;
        }
    }

    /// <summary>
    /// 获取有持仓的交易对集合
    /// </summary>
    /// <returns></returns>
    public HashSet<string> GetSymbolWithPositionSet()
    {
        return m_AppTradingData.PositionInfoMap.Keys.ToHashSet();
    }
    #endregion

    #region 交易历史
    /// <summary>
    /// 获取并汇总指定 symbol 和订单 ID 的交易记录
    /// </summary>
    protected abstract OrderTradeSummaryData GetAndSummarizeTradesImpl(int accountId, string symbol, long orderId);
    #endregion
}