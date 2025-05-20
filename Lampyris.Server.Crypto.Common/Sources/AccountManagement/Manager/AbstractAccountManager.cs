using Lampyris.CSharp.Common;
using CryptoExchange.Net.Clients;

namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 1. ���𱣴�һ��ClientUserId��Ӧ�Ķ��API�˺���Ϣ��ÿ���˺���Ҫ��Ӧһ��APIKey��APISecret��Ϣ
/// 2. �������ӵ�ÿһ��APIKey��Ӧ�����˺ţ�ά��WebSocket��RestAPI Client������
/// 3. ���𱣴����˺���Ϣ��Ӧ���ʲ���Ϣ����, �Ա�ͻ��˶�ȡ
/// 4. ����������˺ŵı仯���ͣ���ʵʱ�������˺��ʲ���Ϣ
/// PS:���˺����ݶ���Ҫʵʱ�����䲻��Ҫ�־û����������ݿ�
/// </summary>
/// <summary>
/// ���࣬����������˻��Ļ�����Ϣ�Ͳ��������漰���Ͳ���
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
    /// ��ȡ�����˻��Ļ�����Ϣ
    /// </summary>
    /// <returns>�˻�������Ϣ�б�</returns>
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
    /// ��ȡ�˻��Ļ�����Ϣ
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>�˻�������Ϣ</returns>
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
    /// �����������˻�(����������Ч��)
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
    /// �����˻�����
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>�Ƿ����ӳɹ�</returns>
    public abstract Task<bool> TestConnectionAsync(int accountId);

    /// <summary>
    /// ��ȡ�˻����ʲ���Ϣ
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>�˻��ʲ���Ϣ</returns>
    public SubTradeAccountSummary GetSubTradeAccountSummary(int accountId)
    {
        return m_SubAccountIdContextDataMap.ContainsKey(accountId) ? 
            m_SubAccountIdContextDataMap[accountId].AccountSummary : null;
    }

    /// <summary>
    /// �����˻������Ϣ
    /// </summary>
    protected abstract void APIUpdateSubTradeAccountInfoImpl();
}

/// <summary>
/// �������࣬������������������ص����˻���Ϣ
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
    /// �����˻�����
    /// </summary>
    /// <param name="accounts">�˻������б�</param>
    public abstract void LoadAccounts(IEnumerable<SubTradeAccount> accounts);

    /// <summary>
    /// ��ȡ�˻���BaseSocketClientʵ��
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>BaseSocketClientʵ��</returns>
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
    /// ��ȡ�˻���BaseRestClientʵ��
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>BaseRestClientʵ��</returns>
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
        // �����ݿ��м������˻��б�
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