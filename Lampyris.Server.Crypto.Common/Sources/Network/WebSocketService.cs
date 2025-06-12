using System.IO.Compression;
using System.Net;
using System.Net.WebSockets;
using Google.Protobuf;
using Lampyris.Crypto.Protocol.App;
using Lampyris.Crypto.Protocol.Common;
using Lampyris.CSharp.Common;

namespace Lampyris.Server.Crypto.Common;

[Component]
public class WebSocketService:ILifecycle
{
    [Autowired("UserDBService")]
    private UserDBService m_UserDBService;

    private Dictionary<int, ClientConnectionInfo> m_UserId2ConnectionInfoMap = new Dictionary<int, ClientConnectionInfo>();

    private MessageHandlerRegistry m_MessageHandlerRegistry = new MessageHandlerRegistry();

    private class ClientConnectionInfo
    {
        public WebSocket WebSocket { get; set; }
        public DateTime ConnectionTime { get; set; }
        public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
        public ClientUserInfo UserInfo { get; set; }
    }

    public override void OnStart()
    {
        m_MessageHandlerRegistry.RegisterHandlers();
    }

    public async Task StartAsync(string url)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        Logger.LogInfo("WebSocket server started, waiting for client to join in...");

        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                WebSocket webSocket = (await context.AcceptWebSocketAsync(null)).WebSocket;
                await HandleWebSocketAsync(webSocket);
            }
        }
    }
    private async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        byte[] buffer = new byte[1024 * 4];
        ClientConnectionInfo clientConnectionInfo = null;

        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }

            // 解压缩并解析 Protobuf 消息
            byte[] decompressedData = Decompress(buffer, result.Count);
            Request request = Request.Parser.ParseFrom(decompressedData);
            if(request.RequestTypeCase == Request.RequestTypeOneofCase.ReqLogin)
            {
                ReqLogin reqLogin = request.ReqLogin;
                ClientUserInfo clientUserInfo = m_UserDBService.QueryClientUserByDeviceMAC(reqLogin.DeviceMAC);
                ResLogin resLogin = new ResLogin();
                resLogin.ErrorMessage = (clientUserInfo == null) ? "Login failed! Unauthorized MAC address" : "";

                // 序列化并压缩响应消息
                await PushMessage(webSocket, resLogin);

                if (clientUserInfo == null)
                {
                    return;
                }

                clientConnectionInfo = m_UserId2ConnectionInfoMap[clientUserInfo.UserId] = new ClientConnectionInfo() 
                {
                    WebSocket = webSocket,
                    ConnectionTime = DateTime.UtcNow,
                    LastHeartbeat = DateTime.UtcNow,
                    UserInfo = clientUserInfo,
                };  
            }
            else if(request.RequestTypeCase == Request.RequestTypeOneofCase.ReqHeartBeat)
            {
                if (clientConnectionInfo != null) {
                    ReqHeartBeat reqHeartBeat = request.ReqHeartBeat;
                    clientConnectionInfo.LastHeartbeat = DateTimeUtil.FromUnixTimestamp(reqHeartBeat.ClientTime);
                }
                else
                {
                    // 非法请求
                }
            }
            else if (request.RequestTypeCase == Request.RequestTypeOneofCase.ReqLogout) // 请求登出
            {
                if (clientConnectionInfo != null)
                {
                    // 从连接信息映射中移除
                    m_UserId2ConnectionInfoMap.Remove(clientConnectionInfo.WebSocket.GetHashCode());

                    // 关闭 WebSocket 连接
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User logged out", CancellationToken.None);

                    Logger.LogInfo($"User logged out: {clientConnectionInfo.WebSocket.GetHashCode()}");
                }
                else
                {
                    Logger.LogWarning("Logout request received, but no valid connection info found.");
                }
            }
            else // 业务协议
            {
                if (clientConnectionInfo != null)
                {
                    if(m_MessageHandlerRegistry.TryGetHandler(request.RequestTypeCase, out var handler))
                    {
                        handler(clientConnectionInfo.UserInfo, request);
                    }
                    else
                    {
                        Logger.LogError($"Unregistered message handler type {request.RequestTypeCase.ToString()}");
                    }
                }
            }
        }
    }

    private async Task PushMessage(WebSocket webSocket, IMessage message)
    {
        // 序列化并压缩响应消息
        byte[] responseData = message.ToByteArray();
        byte[] compressedData = Compress(responseData);
        await webSocket.SendAsync(new ArraySegment<byte>(compressedData), WebSocketMessageType.Binary, true, CancellationToken.None);
    }

    public void PushMessge(int clientUserId, IMessage message)
    {
        ClientConnectionInfo clientConnectionInfo = m_UserId2ConnectionInfoMap[clientUserId];
        if (clientConnectionInfo != null) {
            Task.WaitAny(PushMessage(clientConnectionInfo.WebSocket, message));
        }
    }

    public void PushMessge(ICollection<int> clientUserIds, IMessage message)
    {
        Task[] tasks = new Task[clientUserIds.Count];
        int index = 0;
        foreach (int clientUserId in clientUserIds)
        {
            ClientConnectionInfo clientConnectionInfo = m_UserId2ConnectionInfoMap[clientUserId];
            if (clientConnectionInfo != null)
            {
                tasks[index++] = PushMessage(clientConnectionInfo.WebSocket, message);
            }
        }

        Task.WaitAll(tasks);
    }


    public async void BroadcastMessage(IMessage message)
    {
        foreach(var pair in m_UserId2ConnectionInfoMap)
        {
            await PushMessage(pair.Value.WebSocket, message);
        }
    }

    private byte[] Compress(byte[] data)
    {
        using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
    }

    private byte[] Decompress(byte[] data, int length)
    {
        using (var input = new MemoryStream(data, 0, length))
        using (var gzip = new GZipStream(input, CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }

    public void BroadcastMessage(ResNotice resNotice)
    {
        throw new NotImplementedException();
    }
}
