namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using System.Collections;
using System.IO.Compression;
using System.Text;

[Component]
public class AnnouncementService:AbstractAnnouncementService
{
    public IEnumerator Tick()
    {
        using (HttpClient client = new HttpClient())
        {
            string url = "https://www.binance.com/bapi/apex/v1/public/apex/cms/article/list/query?type=1&pageNo=1&pageSize=50&catalogId=48";
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
            client.DefaultRequestHeaders.Add("Lang", "zh-CN");

            Task<HttpResponseMessage> task = client.GetAsync(url);
            yield return new WaitForTask(task);

            HttpResponseMessage response = task.Result;

            try
            {
                // 确保请求成功
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex.Message);
                yield break;
            }

            // 获取响应的字节数组
            Task<byte[]> task2 = response.Content.ReadAsByteArrayAsync();
            yield return new WaitForTask(task2);
            byte[] responseBytes = task2.Result;

            // 检查是否需要解压缩
            string responseBody;
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                // 解压缩Gzip数据
                using (var compressedStream = new MemoryStream(responseBytes))
                using (var decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressionStream, Encoding.UTF8))
                {
                    responseBody = reader.ReadToEnd();
                }
            }
            else
            {
                // 如果没有压缩，直接解码为字符串
                responseBody = Encoding.UTF8.GetString(responseBytes);
            }


        }
    }
}
