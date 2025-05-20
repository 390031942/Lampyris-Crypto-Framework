using Lampyris.Crypto.Protocol.Common;
using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using System.Reflection;

public class MessageHandlerRegistry
{
    private readonly Dictionary<Request.RequestTypeOneofCase, Action<ClientUserInfo, Request>> m_RequestType2HandlerMap = new();


    public void RegisterHandlers()
    {
        var methods = Components.GetComponentsByTag("MessageHandler")
            .SelectMany(obj => obj.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(method => method.GetCustomAttributes<MessageHandlerAttribute>().Any());

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<MessageHandlerAttribute>();
            if (attribute != null)
            {
                var handlerDelegate = (Action<ClientUserInfo, Request>)Delegate.CreateDelegate(
                    typeof(Action<ClientUserInfo, Request>), method);

                m_RequestType2HandlerMap[attribute.RequestType] = handlerDelegate;
            }
        }
    }

    public bool TryGetHandler(Request.RequestTypeOneofCase requestType, out Action<ClientUserInfo, Request> handler)
    {
        return m_RequestType2HandlerMap.TryGetValue(requestType, out handler);
    }
}
