using Lampyris.CSharp.Common;
using CryptoExchange.Net.Clients;

namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 负责保存一个ClientUserId对应的多个API账号信息，每个账号需要对应一组APIKey和APISecret信息
/// </summary>
[Component]

public abstract class AbstractAccountManager<T> where T: BaseSocketClient
{
    [Autowired]
    protected DBService m_DBService;

    protected readonly Dictionary<int, T> m_SubAccountId2Client; // 存储账户ID与BinanceSocketClient的映射

    protected readonly List<SubTradeAccount> m_AccountConfigs; // 存储账户配置信息

    public AbstractAccountManager()
    {
        m_SubAccountId2Client = new Dictionary<int, T>();
        m_AccountConfigs = new List<SubTradeAccount>();
    }

    /// <summary>
    /// 加载账户配置
    /// </summary>
    /// <param name="accounts">账户配置列表</param>
    public abstract void LoadAccounts(IEnumerable<SubTradeAccount> accounts);

    /// <summary>
    /// 获取账户的BaseSocketClient实例
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>BaseSocketClient实例</returns>
    public T GetClient(int accountId)
    {
        if (m_SubAccountId2Client.TryGetValue(accountId, out var client))
        {
            return client;
        }

        Logger.LogError($"Unabled to find account with id \"{accountId}\"");
        return null;
    }

    /// <summary>
    /// 获取所有账户的基本信息
    /// </summary>
    /// <returns>账户基本信息列表</returns>
    public IEnumerable<SubTradeAccount> GetAllAccounts()
    {
        return m_AccountConfigs.AsReadOnly();
    }

    /// <summary>
    /// 获取账户的基本信息
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>账户基本信息</returns>
    public SubTradeAccount GetAccountInfoById(int accountId)
    {
        var account = m_AccountConfigs.FirstOrDefault(a => a.AccountId == accountId);
        return account;
    }

    /// <summary>
    /// 测试账户连接
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>是否连接成功</returns>
    public abstract bool TestConnectionAsync(int accountId);

    /// <summary>
    /// 获取账户的资产信息
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>账户资产信息</returns>
    public abstract SubTradeAccountSummary GetSubTradeAccountSummary(int accountId);
}
