using Lampyris.Crypto.Protocol.Common;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MessageHandlerAttribute : Attribute
{
    public Request.RequestTypeOneofCase RequestType { get; }

    public MessageHandlerAttribute(Request.RequestTypeOneofCase requestType)
    {
        RequestType = requestType;
    }
}
