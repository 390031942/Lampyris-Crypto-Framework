namespace Lampyris.CSharp.Common;
using Newtonsoft.Json;

public static class JsonUtil
{
    static JsonUtil()
    {
        JsonConvert.DefaultSettings = () =>
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        };
    }
}
