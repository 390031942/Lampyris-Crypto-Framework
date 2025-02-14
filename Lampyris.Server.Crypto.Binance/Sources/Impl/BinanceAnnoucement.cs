using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = "https://www.binance.com/bapi/apex/v1/public/apex/cms/article/list/query?type=1&pageNo=1&pageSize=50&catalogId=48";
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
            client.DefaultRequestHeaders.Add("Lang", "zh-CN");

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                // 确保请求成功
                response.EnsureSuccessStatusCode();

                // 获取响应的字节数组
                byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();

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

                Console.WriteLine("解压后的响应内容：");
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("请求出错：");
                Console.WriteLine(e.Message);
            }
        }
    }
}
