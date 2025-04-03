using System.IO.Compression;
using System.Net;
using System.Net.WebSockets;
using Google.Protobuf;
using Lampyris.Crypto.Protocol.Account;
using Lampyris.Crypto.Protocol.App;
using Lampyris.Crypto.Protocol.Common;
using Lampyris.CSharp.Common;
using ReqLogin = Lampyris.Crypto.Protocol.App.ReqLogin;

namespace Lampyris.Server.Crypto.Common;

public class WebSocketServer
{
    [Autowired("UserDBService")]
    private UserDBService m_UserDBService;

    private Dictionary<int, WebSocket> m_UserId2WebSocketMap = new Dictionary<int, WebSocket>();

    public WebSocketServer()
    {
        
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
                resLogin.ErrorMessage = (clientUserInfo == null) ? "登录失败!未授权的MAC地址" : "";

                // 序列化并压缩响应消息
                byte[] responseData = resLogin.ToByteArray();
                byte[] compressedData = Compress(responseData);
                await webSocket.SendAsync(new ArraySegment<byte>(compressedData), WebSocketMessageType.Binary, true, CancellationToken.None);

                if (clientUserInfo == null)
                {
                    return;
                }

                m_UserId2WebSocketMap[clientUserInfo.UserId] = webSocket;
            }
        }
    }

    private void PushMessage()
    {
        // 序列化并压缩响应消息
        byte[] responseData = resLogin.ToByteArray();
        byte[] compressedData = Compress(responseData);
        await webSocket.SendAsync(new ArraySegment<byte>(compressedData), WebSocketMessageType.Binary, true, CancellationToken.None);
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
}
