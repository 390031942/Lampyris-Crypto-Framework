namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using System.IO.Compression;

[Component]
public class HistoricalDataDownloader
{
    /// <summary>
    /// HTTP对象, 用于请求K线数据
    /// </summary>
    private HttpClient m_HttpClient = new HttpClient();
    
    public async Task<List<QuoteCandleData>> DownloadKlineDataAsync(string symbol, BarSize barSize, DateTime startDate, DateTime endDate)
    {
        startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
        endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);

        string startDateTimeString = startDate.ToString("yyyy-MM-dd");
        List<QuoteCandleData> candleDatas = new List<QuoteCandleData>();

        while (startDate <= endDate)
        {
            string dateTimeString = startDate.ToString("yyyy-MM-dd");
            await DownloadKlineDataAsyncImpl(symbol, barSize.ToParamString(), dateTimeString);
            startDate += TimeSpan.FromDays(1);
        }

        return candleDatas;
    }

    private async Task<List<QuoteCandleData>> DownloadKlineDataAsyncImpl(string symbol, string interval, string date)
    {
        string baseUrl = "https://data.binance.vision/?prefix=data/futures/um/daily/klines/";
        string url = $"{baseUrl}/{symbol}/{interval}/{symbol}-{interval}-{date}.zip";

        string tempFilePath = Path.GetTempFileName();
        string extractFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // 下载 ZIP 文件
            using (var response = await m_HttpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            // 解压 ZIP 文件
            ZipFile.ExtractToDirectory(tempFilePath, extractFolder);

            // 读取 CSV 文件
            string csvFile = Directory.GetFiles(extractFolder, "*.csv")[0];
            var data = new List<QuoteCandleData>();

            foreach (var line in File.ReadAllLines(csvFile))
            {
                try
                {
                    var values = line.Split(',');
                    var candleData = new QuoteCandleData
                    {
                        DateTime = DateTimeUtil.FromUnixTimestamp(long.Parse(values[0])), // OpenTime
                        Open     = double.Parse(values[1]), 
                        High     = double.Parse(values[2]),
                        Low      = double.Parse(values[3]), 
                        Close    = double.Parse(values[4]), 
                        Volume   = double.Parse(values[5]), 
                        Currency = double.Parse(values[7]) 
                    };
                    data.Add(candleData);
                }
                catch { }
            }

            return data;
        } 
        catch
        {
            throw;
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            if (Directory.Exists(extractFolder)) Directory.Delete(extractFolder, true);
        }
    }
}
