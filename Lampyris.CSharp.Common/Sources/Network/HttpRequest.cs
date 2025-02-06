namespace Lampyris.CSharp.Common;

public class HttpRequestExecutor
{
    private readonly HttpClient m_Client = new HttpClient();

    public Task<HttpResponseMessage> GetAsync(string url, Action<string>? callback = null)
    {
        return Task.Run(async () =>
        {
            HttpResponseMessage response = await m_Client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string rawJsonString = await response.Content.ReadAsStringAsync();
                callback?.Invoke(rawJsonString);
            }

            HttpRequest.Recycle(this);
            return response;
        });
    }

    public void GetSync(string url, Action<string>? callback = null)
    {
        HttpResponseMessage response = m_Client.GetAsync(url).Result;
        if (response.IsSuccessStatusCode)
        {
            string rawJsonString = response.Content.ReadAsStringAsync().Result;
            callback?.Invoke(rawJsonString);
        }
    }

    public Task<HttpResponseMessage> PostAsync(string url, StringContent content, Action<string>? callback = null)
    {
        return (Task<HttpResponseMessage>)Task.Run(async () =>
        {
            HttpResponseMessage response = await m_Client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                string rawJsonString = await response.Content.ReadAsStringAsync();
                callback?.Invoke(rawJsonString);
            }

            HttpRequest.Recycle(this);
        });
    }

    public void PostSync(string url, StringContent content, Action<string>? callback = null)
    {
        HttpResponseMessage response = m_Client.PostAsync(url, content).Result;
        if (response.IsSuccessStatusCode)
        {
            string rawJsonString = response.Content.ReadAsStringAsync().Result;
            callback?.Invoke(rawJsonString);
        }
    }
}

public static class HttpRequest
{
    private static readonly Stack<HttpRequestExecutor> ms_HttpRequestExecutors = new Stack<HttpRequestExecutor>();

    private static readonly HttpRequestExecutor m_HttpRequestSync = new HttpRequestExecutor();

    public static void Get(string url, Action<string> callback)
    {
        lock (ms_HttpRequestExecutors)
        {
            if (ms_HttpRequestExecutors.TryPop(out var httpRequest))
            {
                httpRequest.GetSync(url, callback);
            }
            else
            {
                httpRequest = new HttpRequestExecutor();
                httpRequest.GetSync(url, callback);
            }
        }
    }

    public static void GetSync(string url, Action<string>? callback = null)
    {
        m_HttpRequestSync.GetSync(url, callback);
    }

    public static HttpRequestExecutor GetExecutor()
    {
        lock (ms_HttpRequestExecutors)
        {
            if (ms_HttpRequestExecutors.TryPop(out var httpRequest))
            {
                return httpRequest;
            }
            else
            {
                return new HttpRequestExecutor();
            }
        }
    }

    public static void Recycle(HttpRequestExecutor httpRequest)
    {
        lock (ms_HttpRequestExecutors)
        {
            ms_HttpRequestExecutors.Push(httpRequest);
        }
    }
}
