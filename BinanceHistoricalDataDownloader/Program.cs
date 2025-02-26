using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var databaseService = new DatabaseService();
        var binanceApiService = new BinanceApiService();
        var downloadService = new DownloadService();
        var progressManager = new ProgressManager();

        // 加载下载进度
        var progress = progressManager.LoadProgress();

        // 获取所有 USDT 永续合约
        var symbols = await binanceApiService.GetUsdtPerpetualSymbolsAsync();

        // 时间间隔
        var intervals = new[] { "1m", "15m", "1d" };

        // 起始日期
        var startDate = new DateTime(2020, 1, 1);
        var endDate = DateTime.UtcNow;

        // 多线程下载
        var tasks = new List<Task>();
        foreach (var symbol in symbols)
        {
            foreach (var interval in intervals)
            {
                tasks.Add(Task.Run(async () =>
                {
                    string key = $"{symbol.Symbol}_{interval}";
                    DateTime currentDate = progress.ContainsKey(key) ? progress[key].AddDays(1) : startDate;

                    while (currentDate <= endDate)
                    {
                        string date = currentDate.ToString("yyyy-MM-dd");
                        try
                        {
                            Console.WriteLine($"正在下载 {symbol.Symbol} 的 {interval} 数据，日期：{date}");
                            var data = await downloadService.DownloadKlineDataAsync(symbol.Symbol, interval, date);
                            databaseService.CreateTableIfNotExists(symbol.Symbol, interval);
                            databaseService.SaveKlineData(symbol.Symbol, interval, data);

                            // 更新进度
                            progress[key] = currentDate;
                            progressManager.SaveProgress(progress);

                            Console.WriteLine($"已保存 {symbol.Symbol} 的 {interval} 数据，日期：{date}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"下载失败：{symbol.Symbol} 的 {interval} 数据，日期：{date}，错误：{ex.Message}");
                        }

                        currentDate = currentDate.AddDays(1);
                    }
                }));
            }
        }

        await Task.WhenAll(tasks);

        Console.WriteLine("所有数据已处理完成！");
    }
}
