using Lampyris.CSharp.Common;
using CryptoExchange.Net.Clients;

namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 1. 负责保存一个ClientUserId对应的多个API账号信息，每个账号需要对应一组APIKey和APISecret信息
/// 2. 负责连接到每一个APIKey对应的子账号，维护WebSocket和RestAPI Client的连接
/// 3. 负责保存子账号信息对应的资产信息镜像, 以便客户端读取
/// 4. 负责监听子账号的变化推送，以实时更新子账号资产信息
/// PS:子账号数据都需要实时请求，其不需要持久化保存于数据库
/// </summary>
/// <summary>
/// 基类，负责管理子账户的基本信息和操作，不涉及泛型部分
/// </summary>
[Component]
public abstract class AbstractAccountManagerBase:ILifecycle
{
    [Autowired]
    protected AccountDBService m_DBService;

    [Autowired]
    protected AbstractTradingService m_TradeService;

    public override int Priority => 3;
    
    protected class SubTradeAccountContextBase
    {
        public SubTradeAccount AccountInfo = new SubTradeAccount();
        public SubTradeAccountSummary AccountSummary = new SubTradeAccountSummary();
        public bool Connectivity = false;
    }

    protected readonly Dictionary<int, SubTradeAccountContextBase> m_SubAccountIdContextDataMap = new();

    public AbstractAccountManagerBase()
    {
        m_SubAccountIdContextDataMap = new();
    }

    /// <summary>
    /// 获取所有账户的基本信息
    /// </summary>
    /// <returns>账户基本信息列表</returns>
    public IEnumerable<SubTradeAccount> GetAllAccounts()
    {
        foreach (var pair in m_SubAccountIdContextDataMap)
        {
            if(pair.Value.Connectivity)
            {
                yield return pair.Value.AccountInfo;
            }
        }
    }

    /// <summary>
    /// 获取账户的基本信息
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>账户基本信息</returns>
    public SubTradeAccount GetAccountInfoById(int accountId)
    {
        foreach (var pair in m_SubAccountIdContextDataMap)
        {
            if (pair.Value.AccountInfo.AccountId == accountId)
            {
                return pair.Value.AccountInfo;
            }
        }
        return null;
    }

    /// <summary>
    /// 遍历所有子账户(仅限连接有效的)
    /// </summary>
    /// <param name="foreachFunc"></param>
    public void ForeachSubAccount(Action<SubTradeAccount> foreachFunc)
    {
        foreach (var pair in m_SubAccountIdContextDataMap)
        {
            if (pair.Value.Connectivity)
            {
                foreachFunc(pair.Value.AccountInfo);
            }
        }
    }

    /// <summary>
    /// 测试账户连接
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>是否连接成功</returns>
    public abstract Task<bool> TestConnectionAsync(int accountId);

    /// <summary>
    /// 获取账户的资产信息
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>账户资产信息</returns>
    public SubTradeAccountSummary GetSubTradeAccountSummary(int accountId)
    {
        return m_SubAccountIdContextDataMap.ContainsKey(accountId) ? 
            m_SubAccountIdContextDataMap[accountId].AccountSummary : null;
    }

    /// <summary>
    /// 更新账户相关信息
    /// </summary>
    protected abstract void APIUpdateSubTradeAccountInfoImpl();
}

/// <summary>
/// 泛型子类，负责管理与具体类型相关的子账户信息
/// </summary>
[Component]
public abstract class AbstractAccountManager<T, U> : AbstractAccountManagerBase
    where T : BaseSocketClient
    where U : BaseRestClient
{
    protected class SubTradeAccountContext : SubTradeAccountContextBase
    {
        public T SocketClient;
        public U RestClient;
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
    public T GetWebSocketClient(int accountId)
    {
        if (m_SubAccountIdContextDataMap.TryGetValue(accountId, out var context))
        {
            return ((SubTradeAccountContext)context).SocketClient;
        }

        Logger.LogError($"Unable to find account with id \"{accountId}\"");
        return null;
    }

    /// <summary>
    /// 获取账户的BaseRestClient实例
    /// </summary>
    /// <param name="accountId">账户ID</param>
    /// <returns>BaseRestClient实例</returns>
    public U GetRestClient(int accountId)
    {
        if (m_SubAccountIdContextDataMap.TryGetValue(accountId, out var context))
        {
            return ((SubTradeAccountContext)context).RestClient;
        }

        Logger.LogError($"Unable to find account with id \"{accountId}\"");
        return null;
    }

    public override void OnStart()
    {
        // 从数据库中加载子账户列表
        var db = m_DBService.GetTable<SubTradeAccount>();
        if (db == null)
        {
            db = m_DBService.CreateTable<SubTradeAccount>();
        }

        var subAccounts = db.Query();
        if(subAccounts.Count > 0)
        {
            LoadAccounts(subAccounts);
        }
        else
        {
            throw new InvalidDataException("Failed to load any valid sub-account.");
        }
    }
}