using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

/// <summary>
/// 自选管理类，自选的symbol包括自选 + 动态分组
/// 动态分组包括: 主流币，昨日涨幅榜,昨日跌幅榜,昨日成交榜，昨日振幅榜，近7天上新
/// 动态分组会在日期切换的时间点进行更新，并推送给客户端
/// </summary>
[Component]
public class SelfSelectSymbolService:ILifecycle
{
    [Autowired]
    private DBService m_DBService;

    private DBTable<SelfSelectedSymbolGroupData> m_Table;

    /// <summary>
    /// 不可删除的默认动态分组(UserId = -1),不需要存入数据库，但是客户端查询的时候需要追加到列表里
    /// </summary>
    private readonly HashSet<string> m_DynamicGroupName = new HashSet<string>()
    {
        "主流币" ,
        "昨日涨幅榜",
        "昨日跌幅榜",
        "昨日振幅榜",
        "昨日成交榜",
        "近7天上新",
    };

    private readonly List<SelfSelectedSymbolGroupData> m_DynamicGroupData = new List<SelfSelectedSymbolGroupData>();

    public override void OnStart()
    {
        m_Table = m_DBService.GetTable<SelfSelectedSymbolGroupData>();

        foreach(var groupName in m_DynamicGroupName)
        {
            m_DynamicGroupData.Add(new SelfSelectedSymbolGroupData() { Name = groupName });
        }
    }

    public List<SelfSelectedSymbolGroupData> QuerySymbolGroupData(int clientUserId)
    {
        List<SelfSelectedSymbolGroupData> result = new List<SelfSelectedSymbolGroupData>();

        var dbData = m_Table.Query(queryCondition: "clientUserId == @ClientUserId",
                                   parameters: SQLParamMaker.Begin()
                                                            .Append("ClientUserId", clientUserId)
                                                            .End());

        result.AddRange(dbData);
        result.AddRange(m_DynamicGroupData);
        return result;
    }

    public void SetSymbolGroupData(List<SelfSelectedSymbolGroupData> data)
    {
        TaskManager.RunTask("DB Write Self-Selected Symbol Group Data", 10, (progress, token) => {
            progress.Percentage = 0;
            m_Table.Insert(data, update: true);
            progress.Percentage = 100;
        });
    }
}
