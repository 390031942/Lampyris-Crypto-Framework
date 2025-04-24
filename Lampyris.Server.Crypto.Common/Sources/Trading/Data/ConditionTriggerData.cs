using Lampyris.Crypto.Protocol.Trading;

namespace Lampyris.Server.Crypto.Common;

public class ConditionTriggerData
{
    public ConditionOrderTriggerType Type { get; set; }
    public string Value { get; set; }
}
