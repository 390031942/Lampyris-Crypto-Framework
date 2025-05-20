using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

public class DownloadService
{
    private readonly HttpClient _httpClient;

    public DownloadService()
    {
        _httpClient = new HttpClient();
    }

    public static int GetExpectedDataCount(string interval)
    {
        if (interval == "1m")
            return 1440;
        else if (interval == "15m")
            return 1440 / 15;
        else if (interval == "1d")
            return 1;
        return 0;
    }

    public async Task<List<List<object>>> DownloadKlineDataAsync(string symbol, string interval, string date)
    {
        string baseUrl = "https://data.binance.vision/?prefix=data/futures/um/daily/klines/1000000MOGUSDT/1m/";
        string url = $"{baseUrl}/{symbol}/{interval}/{symbol}-{interval}-{date}.zip";

        string tempFilePath = Path.GetTempFileName();
        string extractFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            // 下载 ZIP 文件
            using (var response = await _httpClient.GetAsync(url))
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
            var data = new List<List<object>>();

            foreach (var line in File.ReadAllLines(csvFile))
            {
                try
                {
                    var values = line.Split(',');
                    var row = new List<object>
                    {
                        long.Parse(values[0]), // OpenTime
                        double.Parse(values[1]), // Open
                        double.Parse(values[2]), // High
                        double.Parse(values[3]), // Low
                        double.Parse(values[4]), // Close
                        double.Parse(values[5]), // Volume
                        long.Parse(values[6]), // CloseTime
                        double.Parse(values[7]) // QuoteAssetVolume
                    };
                    data.Add(row);
                }
                catch { }
            }

            return data;
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            if (Directory.Exists(extractFolder)) Directory.Delete(extractFolder, true);
        }
    }
}
