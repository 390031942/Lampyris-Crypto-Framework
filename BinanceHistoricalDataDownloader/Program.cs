using System.Diagnostics;
using System.IO.Pipes;
using System.Diagnostics;
class Program
{
    private static string ms_ErrorFilePath;

    /// <summary>
    /// 等待调试器附加
    /// </summary>
    public static void WaitForDebugger()
    {
        if (!Debugger.IsAttached)
        {
            Console.WriteLine("等待调试器附加...");
            while (!Debugger.IsAttached)
            {
                System.Threading.Thread.Sleep(100); // 每 100 毫秒检查一次
            }
            Console.WriteLine("调试器已附加！");
        }
        else
        {
            Console.WriteLine("调试器已附加，无需等待。");
        }

        // 可选：附加调试器后自动中断到调试器中
        Debugger.Break();
    }

    static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "child")
        {
            // 子进程逻辑
            await RunAsChildProcess(args[1]); // args[1] 是命名管道名称
        }
        else
        {
            // 父进程逻辑
            await RunAsParentProcess();
        }
    }

    /// <summary>
    /// 父进程逻辑
    /// </summary>
    static async Task RunAsParentProcess()
    {
        var binanceApiService = new BinanceApiService();

        // 获取所有 USDT 永续合约
        var symbols = await binanceApiService.GetUsdtPerpetualSymbolsAsync();

        // 将 symbols 列表分成多个子列表
        int processCount = 6; // 启动的子进程数量
        var symbolGroups = SplitSymbols(symbols, processCount);

        // 启动子进程并通过命名管道通信
        var tasks = new List<Task>();
        for (int i = 0; i < processCount; i++)
        {
            int processIndex = i;
            tasks.Add(Task.Run(() => StartChildProcess(symbolGroups[processIndex], processIndex)));
        }

        // 等待所有子进程完成
        await Task.WhenAll(tasks);

        Console.WriteLine("所有子进程已完成！");
    }

    /// <summary>
    /// 将 symbols 列表分成多个子列表
    /// </summary>
    static List<List<SymbolInfo>> SplitSymbols(List<SymbolInfo> symbols, int groupCount)
    {
        var groups = new List<List<SymbolInfo>>();
        int groupSize = (int)Math.Ceiling((double)symbols.Count / groupCount);

        for (int i = 0; i < groupCount; i++)
        {
            groups.Add(symbols.Skip(i * groupSize).Take(groupSize).ToList());
        }

        return groups;
    }

    /// <summary>
    /// 启动子进程并通过命名管道通信
    /// </summary>
    static void StartChildProcess(List<SymbolInfo> symbols, int processIndex)
    {
        string pipeName = $"BinancePipe_{processIndex}";

        // 启动子进程
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName, // 启动当前程序作为子进程
                Arguments = $"child {pipeName}", // 传递命令行参数，标识为子进程
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();

        Console.WriteLine($"子进程 {process.Id} 已启动，处理 {symbols.Count} 个 symbols");

        // 异步读取子进程的标准输出
        Task.Run(() =>
        {
            using (var reader = process.StandardOutput)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine($"子进程 {process.Id}: {line}");
                }
            }
        });

        // 异步读取子进程的错误输出
        Task.Run(() =>
        {
            using (var reader = process.StandardError)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine($"子进程 {process.Id} 错误: {line}");
                }
            }
        });

        // 使用命名管道与子进程通信
        using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
        {
            pipeServer.WaitForConnection();

            // 发送 symbols 数据到子进程
            var writer = new StreamWriter(pipeServer) { AutoFlush = true };
            writer.WriteLine(string.Join(",", symbols.Select(s => s.Symbol)));

            // 读取子进程返回的结果
            var reader = new StreamReader(pipeServer);
            string result;
            while ((result = reader.ReadLine()) != null)
            {
                Console.WriteLine($"子进程{processIndex}(pid = {process.Id}): {result}");
            }
        }

        process.WaitForExit();
    }

    // 异步写入错误日志
    private static async Task LogErrorAsync(string errorMessage)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(ms_ErrorFilePath, append: true))
            {
                await writer.WriteLineAsync($"[{DateTime.UtcNow}] {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"写入错误日志失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 子进程逻辑
    /// </summary>
    static async Task RunAsChildProcess(string pipeName)
    {
        // 使用命名管道与父进程通信
        using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
        {
            pipeClient.Connect();

            using (var writer = new StreamWriter(pipeClient) { AutoFlush = true })
            {
                Console.SetOut(writer);
                // 读取父进程发送的 symbols 数据
                var reader = new StreamReader(pipeClient);
                string symbolsData = await reader.ReadLineAsync();
                var symbols = symbolsData.Split(',').ToList();

                // 执行下载任务
                var downloadService = new DownloadService();
                var databaseService = new DatabaseService();
                var progressManager = new ProgressManager(pipeName);
                var binanceApiService = new BinanceApiService();

                // 加载下载进度
                var progress = progressManager.LoadProgress();

                // 时间间隔
                var intervals = new[] { "1m", "15m", "1d" };

                // 起始日期
                var endDate = DateTime.UtcNow;
                endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);
                endDate = endDate.AddDays(-1);

                ms_ErrorFilePath = $"error_pipeName_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.txt";

                // 获取所有 USDT 永续合约
                var symbolsInfoList = await binanceApiService.GetUsdtPerpetualSymbolsAsync();
                var filterSymbolInfoList = new List<SymbolInfo>();
                foreach (SymbolInfo symbolInfo in symbolsInfoList)
                {
                    if (symbols.Contains(symbolInfo.Symbol))
                    {
                        filterSymbolInfoList.Add(symbolInfo);
                    }
                }

                Dictionary<string, DateTime> filterProcess = new Dictionary<string, DateTime>();
                foreach (var pair in progress)
                {
                    if (symbols.Contains(pair.Key.Split("_")[0]))
                    {
                        filterProcess[pair.Key] = pair.Value;
                    }
                }

                progress = filterProcess;

                for (int i = 0; i < filterSymbolInfoList.Count; i++)
                {
                    var symbolInfo = filterSymbolInfoList[i];
                    var startDate = new DateTime(2020, 1, 1, 0, 0, 0);
                    var onBoardDate = DateTimeOffset.FromUnixTimeMilliseconds(symbolInfo.OnboardDate).UtcDateTime;
                    onBoardDate = new DateTime(onBoardDate.Year, onBoardDate.Month, onBoardDate.Day);

                    if ((onBoardDate - startDate).TotalDays > 0)
                    {
                        startDate = onBoardDate;
                    }

                    var symbol = symbolInfo.Symbol;

                    foreach (var interval in intervals)
                    {
                        string key = $"{symbol}_{interval}";
                        DateTime currentDate = progress.ContainsKey(key) ? progress[key].AddDays(1) : startDate;

                        while (currentDate < endDate)
                        {
                            try
                            {
                                string date = currentDate.ToString("yyyy-MM-dd");

                                Console.WriteLine($"[{i + 1}/{symbols.Count}] 正在下载 {symbol}...数据，日期：{date}");
                                var data = await downloadService.DownloadKlineDataAsync(symbol, interval, date);
                                databaseService.CreateTableIfNotExists(symbol, interval);
                                databaseService.SaveKlineData(symbol, interval, data);

                                // 更新进度
                                progress[key] = currentDate;
                                progressManager.SaveProgress(progress);

                                Console.WriteLine($"[{i + 1}/{symbols.Count}] 已保存 {symbol}的 {interval} 数据，日期：{date}");

                            }
                            catch (Exception ex)
                            {
                                string errorMsg = $"[{i + 1}/{symbols.Count}] 处理 {symbol} 失败: {ex.Message}";
                                Console.WriteLine(errorMsg);

                                // 写入 error.txt
                                await LogErrorAsync(errorMsg);
                            }
                            finally
                            {
                                currentDate = currentDate.AddDays(1);
                            }
                        }
                    }
                }
                Console.WriteLine($"子进程{pipeName}完成任务");
            }
        }
    }
}
