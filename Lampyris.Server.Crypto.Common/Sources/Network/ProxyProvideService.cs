namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;

/// <summary>
/// VPN代理信息
/// </summary>
public class ProxyInfoJsonObject
{
    /// <summary>
    /// 最小的VPN代理 要求数量，小于这个程序将无法启动
    /// </summary>
    public int             MinimumRequirement = 3;

    /// <summary>
    /// 代理信息
    /// </summary>
    public List<ProxyInfo> ProxyInfos = new List<ProxyInfo>();

    /// <summary>
    /// 是否可用
    /// </summary>
    [JsonIgnore]
    public bool            Valid = false;
}

public class ProxyInfo
{
    public string Address;
    public int    Port;
}

[Component]
public class ProxyProvideService:ILifecycle
{
    private ProxyInfoJsonObject m_ProxyInfoObject;

    private const string ProxyInfoObjectSaveFilePath = "Network/json_settings.json";

    public override int Priority => 1;

    public override void OnStart()
    {
        Logger.LogInfo($"Loading proxy settings from \"{ProxyInfoObjectSaveFilePath}\"..., and then reachable test will be made...");
        m_ProxyInfoObject = JsonConvert.DeserializeObject<ProxyInfoJsonObject>(ProxyInfoObjectSaveFilePath);
        
        if(m_ProxyInfoObject == null)
        {
            throw new InvalidDataException("Cannot load proxy settings...");
        }

        if(m_ProxyInfoObject.ProxyInfos.Count < m_ProxyInfoObject.MinimumRequirement)
        {
            throw new InvalidDataException($"The length of proxy settings doesn't satisfy the minimum requirement {m_ProxyInfoObject.MinimumRequirement}"); 
        }

        int reachableCount = 0;
        foreach(ProxyInfo proxyInfo in m_ProxyInfoObject.ProxyInfos)
        {
            if (TestReachable(proxyInfo))
            {
                reachableCount++;
            }
        }

        if (reachableCount < m_ProxyInfoObject.MinimumRequirement)
        {
            throw new InvalidDataException($"The length of reachable proxy settings doesn't satisfy the minimum requirement {m_ProxyInfoObject.MinimumRequirement}");
        }
    }

    private static bool TestReachable(ProxyInfo proxyInfo)
    {
        bool result = false;
        // 代理服务器地址和端口
        // 使用 TcpClient 测试端口连通性
        using (var tcpClient = new TcpClient())
        {
            try
            {
                tcpClient.Connect(proxyInfo.Address, proxyInfo.Port);
                result = true;
            }
            catch { }
        }

        Logger.LogInfo($"Reachable test result \"{proxyInfo.Address}:{proxyInfo.Port}\": {(result ? "Reachable": "UnReachable")}");
        return result;
    }

    public int ProxyCount
    {
        get
        {
            return m_ProxyInfoObject?.ProxyInfos.Count ?? 0;
        }
    }

    public ProxyInfo Get(int index)
    {
        return (m_ProxyInfoObject != null && index >= 0 && index < ProxyCount) ? m_ProxyInfoObject.ProxyInfos[index] : null;
    }
}
