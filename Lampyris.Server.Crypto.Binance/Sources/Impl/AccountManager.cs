namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using CryptoExchange.Net.Authentication;
using Lampyris.Crypto.Protocol.App;
using global::Binance.Net.Clients;
using global::Binance.Net.Objects.Models.Futures.Socket;
using global::Binance.Net.Objects.Models.Futures;

[Component]

public class AccountManager: AbstractAccountManager<BinanceSocketClient,BinanceRestClient>
{
    [Autowired]
    private WebSocketService m_WebSocketService;

    /// <summary>
    /// 子账户ID对应的Binance API账户事件监听listenKey
    /// </summary>
    private Dictionary<int, string> m_UserId2ListenKeyMap = new ();

    /// <summary>
    /// 待重新连接的子账户列表
    /// </summary>
    private Dictionary<string, SubTradeAccountContext> m_WaitForRetryMap = new();

    public async Task SubscribeToUserDataUpdatesAsync(int accountId, BinanceRestClient restClient, BinanceSocketClient webSocketClient, string listenKey)
    {
        // 订阅用户数据更新
        var subscriptionResult = await webSocketClient.UsdFuturesApi.Account.SubscribeToUserDataUpdatesAsync(
            listenKey,
            // 杠杆更新事件回调
            onLeverageUpdate: leverageUpdate =>
            {
                if (leverageUpdate.Data != null && leverageUpdate.Data.LeverageUpdateData != null)
                {
                    var data = leverageUpdate.Data.LeverageUpdateData;
                    m_TradeService.AccountUpdateLeverage(accountId, data.Symbol, data.Leverage);
                    Logger.LogInfo($"Lererage of sub-account id = {accountId}, Symbol = {data.Symbol} changed to {data.Leverage}");
                }
            },
            // 保证金追加事件回调
            onMarginUpdate: marginUpdate =>
            {
                if (marginUpdate.Data != null)
                {
                    m_WebSocketService.BroadcastMessage(new ResNotice()
                    {
                        Content = "",
                        Type = NoticeType.AlertDialog,
                    });
                }
            },
            // 账户更新事件回调
            onAccountUpdate: accountUpdate =>
            {
                if (accountUpdate.Data != null)
                {
                    // 资产信息仅仅更新USDT
                    foreach (var balance in accountUpdate.Data.UpdateData.Balances)
                    {
                        if(balance != null && balance.Asset == "USDT")
                        {
                            Logger.LogInfo($"Available balance = {balance.WalletBalance}");
                        }
                    }

                    // 有持仓的交易对集合
                    HashSet<string> symbols = m_TradeService.GetSymbolWithPositionSet();

                    // 更新持仓信息
                    foreach(var apiPosition in accountUpdate.Data.UpdateData.Positions)
                    {
                        // 这里出现的symbol移除掉，最终得到的symbol集合就是清仓了的
                        symbols.Remove(apiPosition.Symbol);
                        PositionUpdateInfo updateInfo = Converter.ToPositionUpdateInfo(apiPosition);
                        m_TradeService.UpdatePosition(accountId, updateInfo);
                    }

                    // 更新已清仓的信息
                    foreach(string symbol in symbols)
                    {
                        m_TradeService.SetClearedPositionForSymbol(accountId, symbol);
                    }
                }
            },
            // 订单更新事件回调
            onOrderUpdate: orderUpdate =>
            {
                if (orderUpdate.Data != null && orderUpdate.Data.UpdateData != null)
                {
                    var rawOrderStatusData = orderUpdate.Data.UpdateData;
                    OrderStatusInfo orderStatusInfo = Converter.ToOrderStatusInfo(rawOrderStatusData);
                    m_TradeService.UpdateOrderStatus(accountId, orderStatusInfo);
                }
            },
            // 交易更新事件回调
            onTradeUpdate: tradeUpdate =>
            {
                if(tradeUpdate.Data != null)
                {
                    BinanceFuturesStreamTradeUpdate update = tradeUpdate.Data;
                    // m_TradeService.RecordTrade(accountId, tradeUpdate.Data);
                }
            },
            // ListenKey过期事件回调
            onListenKeyExpired: listenKeyExpired =>
            {
                Logger.LogInfo($"ListenKey for account id {accountId} expired, try to re-obtain");
                // 在这里处理ListenKey过期逻辑
                var listenKeyResult = restClient.UsdFuturesApi.Account.StartUserStreamAsync().Result;
                if (!listenKeyResult.Success)
                {
                    Logger.LogError($"Failed to obtain Listen Key：{listenKeyResult.Error?.Message}");
                }
            },
            // 策略更新事件回调
            onStrategyUpdate: null,
            // 网格更新事件回调
            onGridUpdate: null,
            // 条件订单触发拒绝事件回调
            onConditionalOrderTriggerRejectUpdate: conditionalOrderTriggerRejectUpdate =>
            {
                if (conditionalOrderTriggerRejectUpdate.Data != null && conditionalOrderTriggerRejectUpdate.Data.RejectInfo != null)
                {
                    var data = conditionalOrderTriggerRejectUpdate.Data.RejectInfo;
                    Logger.LogWarning($"Conditional order rejected, account id = {accountId}, orderId = : {data.OrderId}, reason = : {data.Reason}");
                }
            },
            ct: CancellationToken.None
        );

        // 检查订阅是否成功
        if (!subscriptionResult.Success)
        {
            Logger.LogInfo($"订阅失败: {subscriptionResult.Error}");
        }
        else
        {
            Logger.LogInfo("订阅成功！");
        }
    }

    [Autowired]
    private ProxyProvideService m_ProxyProvideService;

    /// <summary>
    /// 加载账户配置,创建并初始化WebSocket和Rest Client对象
    /// </summary>
    /// <param name="accounts">账户配置列表</param>
    public override void LoadAccounts(IEnumerable<SubTradeAccount> accounts)
    {
        foreach (var account in accounts)
        {
            if (!m_SubAccountIdContextDataMap.ContainsKey(account.AccountId))
            {
                var subTradeAccountContext = new SubTradeAccountContext();
                var apiCredentitals = new ApiCredentials(account.ApiKey, account.ApiSecret);
                
                // Client
                var proxyInfo = m_ProxyProvideService.Get(0);
                if (proxyInfo == null)
                {
                    throw new InvalidProgramException("Failed to allocate create REST client: proxy info is invalid");
                }
                BinanceRestClient restClient = new BinanceRestClient();

                restClient.SetApiCredentials(apiCredentitals);
                // 获取 Listen Key
                var listenKeyResult = restClient.UsdFuturesApi.Account.StartUserStreamAsync().Result;
                if (!listenKeyResult.Success)
                {
                    Logger.LogInfo($"Failed to obtain Listen Key：{listenKeyResult.Error?.Message}");
                    continue;
                }

                string listenKey = listenKeyResult.Data;
                m_UserId2ListenKeyMap[account.AccountId] = listenKey;

                // WebSocket
                var webSocketClient = new BinanceSocketClient();
                webSocketClient.SetApiCredentials(apiCredentitals);


                subTradeAccountContext.RestClient = restClient;
                subTradeAccountContext.SocketClient = webSocketClient;
                subTradeAccountContext.AccountInfo = account;
                m_SubAccountIdContextDataMap[account.AccountId] = subTradeAccountContext;
                subTradeAccountContext.Connectivity = TestConnectionAsync(account.AccountId).Result;

                // 初始化拥有的资产信息
                // 绑定资产更新事件
                Task.Run(async() => await SubscribeToUserDataUpdatesAsync(account.AccountId, restClient, webSocketClient, listenKey));
            }
        }
    }

    /// <summary>
    /// 获取账户的资产信息
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>账户资产信息</returns>
    public async Task<BinanceFuturesAccountInfoV3> GetAccountAssetsAsync(int accountId)
    {
        var client = GetRestClient(accountId);
        var accountInfoResult = await client.UsdFuturesApi.Account.GetAccountInfoV3Async();

        if (!accountInfoResult.Success)
        {
            throw new Exception($"Failed to : {accountInfoResult.Error?.Message}");
        }
        
        return accountInfoResult.Data;
    }

    public override async Task<bool> TestConnectionAsync(int accountId)
    {
        var client = GetRestClient(accountId);
        var pingAsync = client.UsdFuturesApi.ExchangeData.PingAsync();
        await pingAsync;
        return pingAsync.Result.Success; 
    }


    /// <summary>
    /// 根据 Binance 合约的手续费规则，获取手续费级别的万分比值
    /// </summary>
    /// <param name="feeTier">手续费等级（0-9）</param>
    /// <param name="isMaker">是否为挂单（Maker）。如果为吃单（Taker），传入 false</param>
    /// <returns>手续费级别的万分比值</returns>
    public decimal GetFeeRate(int feeTier, bool isMaker)
    {
        // Binance 合约手续费规则映射表
        var feeRateMap = new Dictionary<int, (decimal MakerFee, decimal TakerFee)>
        {
            { 0, (0.0200m, 0.0500m) }, // Regular User
            { 1, (0.0160m, 0.0400m) }, // VIP 1
            { 2, (0.0140m, 0.0350m) }, // VIP 2
            { 3, (0.0120m, 0.0320m) }, // VIP 3
            { 4, (0.0100m, 0.0300m) }, // VIP 4
            { 5, (0.0080m, 0.0270m) }, // VIP 5
            { 6, (0.0060m, 0.0250m) }, // VIP 6
            { 7, (0.0040m, 0.0220m) }, // VIP 7
            { 8, (0.0020m, 0.0200m) }, // VIP 8
            { 9, (0.0000m, 0.0170m) }  // VIP 9
        };

        // 检查 feeTier 是否在映射表范围内
        if (!feeRateMap.ContainsKey(feeTier))
        {
            Logger.LogError("Fee tier must be between 0 and 9.");
            return 0m;
        }

        // 根据 isMaker 返回对应的手续费级别
        var (makerFee, takerFee) = feeRateMap[feeTier];
        return isMaker ? makerFee : takerFee;
    }

    /// <summary>
    /// 获取子交易账户的摘要信息
    /// </summary>
    /// <param name="accountId">账户 ID</param>
    /// <returns>子交易账户摘要信息</returns>
    protected override void APIUpdateSubTradeAccountInfoImpl()
    {
        foreach (var pair in m_SubAccountIdContextDataMap)
        {
            int accountId = pair.Key;
            var client = ((SubTradeAccountContext)pair.Value).RestClient;

            // 调用 Binance API 获取账户信息
            var accountInfoResponse = client.UsdFuturesApi.Account.GetAccountInfoV2Async().Result;

            if (!accountInfoResponse.Success || accountInfoResponse.Data == null)
            {
                Logger.LogError($"Failed to fetch account info for accountId: {accountId}. Error: {accountInfoResponse.Error?.Message}");
            }
            else
            {
                var accountInfo = accountInfoResponse.Data;

                // 构造 SubTradeAccountSummary 对象
                var summary = new SubTradeAccountSummary
                {
                    CanDeposit = accountInfo.CanDeposit,
                    CanTrade = accountInfo.CanTrade,
                    CanWithdraw = accountInfo.CanWithdraw,
                    MakerFee = GetFeeRate(accountInfo.FeeTier, true),
                    TakerFee = GetFeeRate(accountInfo.FeeTier, false),
                    MaxWithdrawQuantity = accountInfo.MaxWithdrawQuantity,
                    TotalInitialMargin = accountInfo.TotalInitialMargin.ToString(),
                    TotalMaintMargin = accountInfo.TotalMaintMargin.ToString(),
                    TotalMarginBalance = accountInfo.TotalMarginBalance.ToString(),
                    TotalOpenOrderInitialMargin = accountInfo.TotalOpenOrderInitialMargin.ToString(),
                    TotalPositionInitialMargin = accountInfo.TotalPositionInitialMargin.ToString(),
                    TotalUnrealizedProfit = accountInfo.TotalUnrealizedProfit.ToString(),
                    TotalWalletBalance = accountInfo.TotalWalletBalance.ToString(),
                    AvailableBalance = accountInfo.AvailableBalance.ToString(),
                    UpdateTime = accountInfo.UpdateTime
                };
            }
            // 获取ADL级别
            var adlResponse = client.UsdFuturesApi.Account.GetPositionAdlQuantileEstimationAsync().Result;
            if (!adlResponse.Success || adlResponse.Data == null)
            {
                Logger.LogError($"Failed to fetch adl level for accountId: {accountId}. Error: {accountInfoResponse.Error?.Message}");
            }
            else
            {
                foreach (var data in adlResponse.Data)
                {
                    m_TradeService.UpdateADLLevel(accountId, data.Symbol, data.AdlQuantile?.Both ?? 0);
                }
            }
        }
    }
}