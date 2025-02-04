namespace Lampyris.Framework.Server.Common;

using System.Net.Sockets;

public class ClientEntity
{
    // 客户端连接的ID
    public long ID { get; private set; }
    
    // 客户端连接时间
    public long ConnectedTimestamp { get; private set; }
    
    // 上次收到客户端心跳包时间
    public long LastHeartBeatTimestamp { get; private set; }
    
    // 客户端Socket对象
    private Socket m_Socket;
}