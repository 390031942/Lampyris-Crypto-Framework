namespace Lampyris.Server.Crypto.Common;

using Lampyris.CSharp.Common;
using System.Collections.Generic;

public class WaitForQuoteCandleResult : AsyncOperation
{
    private Task<HttpResponseMessage> m_Task;

    public List<QuoteCandleData> GetResult()
    {
        if (m_Task.Result.IsSuccessStatusCode)
        {
            string json = m_Task.Result.Content.ReadAsStringAsync().Result;
            return OkxResponseJsonParser.ParseCandleList(json);
        }

        return null;
    }

    public WaitForQuoteCandleResult(string url)
    {
        m_Task = HttpRequest.GetExecutor().GetAsync(url);
    }

    public override bool MoveNext()
    {
        return m_Task.IsCompleted;
    }
}
