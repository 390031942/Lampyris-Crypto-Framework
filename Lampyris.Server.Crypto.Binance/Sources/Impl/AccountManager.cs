using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;

[Component]

public class AccountManager: AbstractAccountManager<BinanceSocketClient>
{
    /// <summary>
    /// 加载账户配置
    /// </summary>
    /// <param name="accounts">账户配置列表</param>
    public override void LoadAccounts(IEnumerable<SubTradeAccount> accounts)
    {
        m_AccountConfigs.Clear();
        m_AccountConfigs.AddRange(accounts);

        foreach (var account in accounts)
        {
            if (!m_SubAccountId2Client.ContainsKey(account.AccountId))
            {
                var client = new BinanceSocketClient();
                client.SetApiCredentials(new ApiCredentials(account.ApiKey, account.ApiSecret));
                m_SubAccountId2Client.Add(account.AccountId, client);
            }
        }
    }

    /// <summary>
    /// 获取账户的资产信息
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>账户资产信息</returns>
    public async Task<Binance.Net.Objects.Models.Futures.BinanceFuturesAccountInfo> GetAccountAssetsAsync(int accountId)
    {
        var client = GetClient(accountId);
        var accountInfoResult = await client.FuturesUsdt.Account.GetAccountInfoAsync();

        if (!accountInfoResult.Success)
        {
            throw new Exception($"获取账户资产信息失败: {accountInfoResult.Error?.Message}");
        }

        return accountInfoResult.Data;
    }

    public override Task<bool> TestConnectionAsync(int accountId)
    {
        var client = GetClient(accountId);
        var pingResult = client.UsdFuturesApi.ExchangeData.ping().Result;

        return pingResult.Success;
    }

    public override SubTradeAccountSummary GetSubTradeAccountSummary(int accountId)
    {
        var client = GetClient(accountId);
        client.UsdFuturesApi.Account.SubscribeToUserDataUpdatesAsync()
        throw new NotImplementedException();
    }

    public async Task ListenToAccountUpdatesAsync(CancellationToken cancellationToken)
    {
        var client = GetClient(accountId);
        try
        {
            // 1. 获取 Listen Key
            var listenKeyResult = await _restClient.UsdFuturesApi.Account.StartUserStreamAsync(cancellationToken);
            if (!listenKeyResult.Success)
            {
                Console.WriteLine($"获取 Listen Key 失败：{listenKeyResult.Error?.Message}");
                return;
            }

            string listenKey = listenKeyResult.Data;
            Console.WriteLine($"成功获取 Listen Key：{listenKey}");

            // 2. 订阅账户更新流
            var subscriptionResult = await _socketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(
                listenKey,
                onAccountUpdate: data =>
                {
                    // 处理账户更新事件
                    UpdateAccountSummary(data.Data);
                },
                onMarginUpdate: data =>
                {
                    // 处理持仓更新事件
                    UpdatePositions(data.Data);
                },
                onListenKeyExpired: data =>
                {
                    // 处理 Listen Key 过期事件
                    Console.WriteLine("Listen Key 已过期，请重新启动数据流。");
                },
                ct: cancellationToken
            );

            if (!subscriptionResult.Success)
            {
                Console.WriteLine($"订阅账户更新流失败：{subscriptionResult.Error?.Message}");
                return;
            }

            Console.WriteLine("成功订阅账户更新流！");

            // 等待取消订阅
            await Task.Delay(Timeout.Infinite, cancellationToken);

            // 取消订阅
            await _socketClient.UnsubscribeAsync(subscriptionResult.Data);
            Console.WriteLine("已取消订阅账户更新流。");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("账户更新监听已取消。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"监听账户更新时发生异常：{ex.Message}");
        }
    }

}