namespace Lampyris.CSharp.Common;

using System.Collections;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class WaitForHttpResponse : IEnumerator
{
    private Task<HttpResponseMessage> m_Task;
    private HttpResponseMessage?      m_Response;
    private HttpRequestExecutor       m_Client;

    public WaitForHttpResponse(string url)
    {
        m_Client = HttpRequest.GetTemp();
        m_Task = m_Client.GetAsync(url);
    }

    public WaitForHttpResponse(string url, string content, string? mediaType = "application/json")
    {
        m_Client = HttpRequest.GetTemp();
        var requestBody = new StringContent(content, Encoding.UTF8, mediaType);
        m_Task = m_Client.PostAsync(url, requestBody);
    }

    public bool MoveNext()
    {
        if (m_Task.IsCompleted)
        {
            m_Response = m_Task.Result;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        m_Client = HttpRequest.GetTemp();
    }

    public object? Current => m_Response;

    public string Result
    {
        get
        {
            if (m_Task != null && m_Task.IsCompleted)
            {
                return m_Task.Result.Content.ReadAsStringAsync().Result;
            }
            return "";
        }
    }
}