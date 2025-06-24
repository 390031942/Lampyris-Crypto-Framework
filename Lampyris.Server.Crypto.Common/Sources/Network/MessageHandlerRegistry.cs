using Lampyris.Crypto.Protocol.Common;
using Lampyris.CSharp.Common;
using System.Reflection;

public class MessageHandlerRegistry
{
    private readonly Dictionary<Request.RequestTypeOneofCase, Action<int, Request>> m_RequestType2HandlerMap = new();


    public void RegisterHandlers()
    {
        var handlers = Components.GetComponentsByTag("MessageHandler");

        foreach (var handler in handlers)
        {
            var methods = handler.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes<MessageHandlerAttribute>().Any());

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<MessageHandlerAttribute>();
                if (attribute != null)
                {
                    var handlerDelegate = (Action<int, Request>)Delegate.CreateDelegate(
                        typeof(Action<int, Request>), handler, method);

                    m_RequestType2HandlerMap[attribute.RequestType] = handlerDelegate;
                }
            }
        }
    }

    public bool TryGetHandler(Request.RequestTypeOneofCase requestType, out Action<int, Request> handler)
    {
        return m_RequestType2HandlerMap.TryGetValue(requestType, out handler);
    }
}
