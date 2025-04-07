using Lampyris.CSharp.Common;
using CryptoExchange.Net.Clients;

namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// ���𱣴�һ��ClientUserId��Ӧ�Ķ��API�˺���Ϣ��ÿ���˺���Ҫ��Ӧһ��APIKey��APISecret��Ϣ
/// </summary>
[Component]

public abstract class AbstractAccountManager<T> where T: BaseSocketClient
{
    [Autowired]
    protected DBService m_DBService;

    protected readonly Dictionary<int, T> m_SubAccountId2Client; // �洢�˻�ID��BinanceSocketClient��ӳ��

    protected readonly List<SubTradeAccount> m_AccountConfigs; // �洢�˻�������Ϣ

    public AbstractAccountManager()
    {
        m_SubAccountId2Client = new Dictionary<int, T>();
        m_AccountConfigs = new List<SubTradeAccount>();
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
    /// ��ȡ�����˻��Ļ�����Ϣ
    /// </summary>
    /// <returns>�˻�������Ϣ�б�</returns>
    public IEnumerable<SubTradeAccount> GetAllAccounts()
    {
        return m_AccountConfigs.AsReadOnly();
    }

    /// <summary>
    /// ��ȡ�˻��Ļ�����Ϣ
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>�˻�������Ϣ</returns>
    public SubTradeAccount GetAccountInfoById(int accountId)
    {
        var account = m_AccountConfigs.FirstOrDefault(a => a.AccountId == accountId);
        return account;
    }

    /// <summary>
    /// �����˻�����
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>�Ƿ����ӳɹ�</returns>
    public abstract bool TestConnectionAsync(int accountId);

    /// <summary>
    /// ��ȡ�˻����ʲ���Ϣ
    /// </summary>
    /// <param name="accountId">�˻�ID</param>
    /// <returns>�˻��ʲ���Ϣ</returns>
    public abstract SubTradeAccountSummary GetSubTradeAccountSummary(int accountId);
}
