namespace Lampyris.Server.Crypto.Binance;

using Lampyris.CSharp.Common;
using Lampyris.Server.Crypto.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Component]
public class MarketDataService
{
    [Autowired]
    private ProxyProvideService m_ProxyProvideService;

    private Dictionary<string, QuoteTickerData> m_QuoteTickerDataMap = new();

    private QuoteTickerData GetOrCreateTickerData(string symbol)
    {
        if (m_QuoteTickerDataMap.ContainsKey(symbol))
        {
            return m_QuoteTickerDataMap[symbol];
        }

        var data = new QuoteTickerData() { Symbol = symbol };
        m_QuoteTickerDataMap.Add(symbol, data);

        return data;
    }

    /// <summary>
    /// 全体USDT永续合约symbol
    /// </summary>
    private HashSet<string> m_AllSymbolSet = new HashSet<string>();

    public IReadOnlyCollection<string> GetAllSymbols()
    {
        return m_AllSymbolSet;
    }

    public void Init()
    {
        ProxyInfo proxyInfo = m_ProxyProvideService.AllocateProxy();
        MarketDataWebSocketClient webSocketClient = new MarketDataWebSocketClient(proxyInfo);

        // 订阅行情
        Task.WaitAll(webSocketClient.SubscribeTicker());

        webSocketClient.OnMessageReceived += MessageHandler;
    }

    /// <summary>
    /// 处理 Binance WebSocket 推送的消息
    /// </summary>
    /// <param name="message">WebSocket 接收到的 JSON 消息</param>
    public void MessageHandler(string message)
    {
        try
        {
            // 将消息解析为 JSON 对象
            var json = JObject.Parse(message);

            // 检查消息类型
            if (json.ContainsKey("e")) // "e" 表示事件类型
            {
                string eventType = json["e"].ToString(); // 事件类型，例如 "aggTrade"、"kline" 等

                switch (eventType)
                {
                    case "aggTrade": // 聚合交易数据
                        HandleAggTrade(json);
                        break;

                    case "markPriceUpdate": // 标记价格更新
                        HandleMarkPriceUpdate(json);
                        break;

                    case "kline": // K线数据
                        HandleKline(json);
                        break;

                    case "24hrTicker": // 24小时价格变动统计
                        Handle24hrTicker(json);
                        break;

                    case "depthUpdate": // 深度更新
                        HandleDepthUpdate(json);
                        break;

                    default:
                        Console.WriteLine($"未处理的事件类型: {eventType}");
                        break;
                }
            }
            else
            {
                Console.WriteLine("未知消息格式: " + message);
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON 解析错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"处理消息时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理聚合交易数据 (aggTrade)
    /// </summary>
    private void HandleAggTrade(JObject json)
    {
        var trade = new
        {
            EventTime = json["E"]?.ToObject<long>(), // 事件时间戳（毫秒）
            Symbol = json["s"]?.ToString(), // 交易对符号，例如 "BTCUSDT"
            TradeId = json["a"]?.ToObject<long>(), // 聚合交易 ID
            Price = json["p"]?.ToObject<decimal>(), // 成交价格
            Quantity = json["q"]?.ToObject<decimal>(), // 成交数量
            BuyerMaker = json["m"]?.ToObject<bool>() // 是否为主动卖方成交（true 表示卖方主动）
        };

        Console.WriteLine($"[AggTrade] Symbol: {trade.Symbol}, Price: {trade.Price}, Quantity: {trade.Quantity}");
    }

    /// <summary>
    /// 处理标记价格更新 (markPriceUpdate)
    /// </summary>
    private void HandleMarkPriceUpdate(JObject json)
    {
        string symbol = json["s"].ToString(); // 交易对符号，例如 "BTCUSDT"
        var data = GetOrCreateTickerData(symbol);

        data.MarkPrice = json["p"].ToObject<double>(); // 标记价格
        data.FundingRate = json["r"].ToObject<double>(); // 当前资金费率
        data.NextFundingTime = json["T"].ToObject<long>(); // 下次资金费率结算时间（毫秒）
    }

    /// <summary>
    /// 处理 K 线数据 (kline)
    /// </summary>
    private void HandleKline(JObject json)
    {
        var kline = json["k"]; // K 线数据对象
        var klineData = new
        {
            Symbol = json["s"]?.ToString(), // 交易对符号，例如 "BTCUSDT"
            StartTime = kline?["t"]?.ToObject<long>(), // K 线开始时间（毫秒）
            CloseTime = kline?["T"]?.ToObject<long>(), // K 线结束时间（毫秒）
            OpenPrice = kline?["o"]?.ToObject<decimal>(), // 开盘价
            ClosePrice = kline?["c"]?.ToObject<decimal>(), // 收盘价
            HighPrice = kline?["h"]?.ToObject<decimal>(), // 最高价
            LowPrice = kline?["l"]?.ToObject<decimal>(), // 最低价
            Volume = kline?["v"]?.ToObject<decimal>(), // 成交量
            IsFinal = kline?["x"]?.ToObject<bool>() // 是否为最终 K 线（true 表示已结束）
        };

        Console.WriteLine($"[Kline] Symbol: {klineData.Symbol}, Open: {klineData.OpenPrice}, Close: {klineData.ClosePrice}");
    }

    /// <summary>
    /// 处理 24 小时价格变动统计 (24hrTicker)
    /// </summary>
    private void Handle24hrTicker(JObject json)
    {
        try
        {
            JArray array = json.ToObject<JArray>();
            if(array != null)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    JObject jObject = array.ElementAt(i).ToObject<JObject>();

                    string symbol = jObject["s"].ToString(); // 交易对符号，例如 "BTCUSDT"
                    var data = GetOrCreateTickerData(symbol);

                    data.Change     = jObject["p"].ToObject<double>(); // 价格变动值
                    data.ChangePerc = jObject["P"].ToObject<double>(); // 价格变动百分比
                    data.High       = jObject["h"].ToObject<double>(); // 24 小时内最高价
                    data.Low        = jObject["l"].ToObject<double>(); // 24 小时内最高价
                    data.Price      = jObject["c"].ToObject<double>(); // 现价
                    data.Volumn     = jObject["v"].ToObject<double>(); // 24 小时内成交量
                    data.Currency   = jObject["q"].ToObject<double>(); // 24 小时内成交额
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error occurred while parsing 24hrTicker, reason: {ex.Message}.");
        }
    }

    /// <summary>
    /// 处理深度更新 (depthUpdate)
    /// </summary>
    private void HandleDepthUpdate(JObject json)
    {
        var depth = new
        {
            Symbol = json["s"]?.ToString(), // 交易对符号，例如 "BTCUSDT"
            UpdateId = json["u"]?.ToObject<long>(), // 更新 ID
            Bids = json["b"]?.ToObject<JArray>(), // 买单深度列表
            Asks = json["a"]?.ToObject<JArray>() // 卖单深度列表
        };

        Console.WriteLine($"[DepthUpdate] Symbol: {depth.Symbol}, Bids: {depth.Bids.Count}, Asks: {depth.Asks.Count}");
    }
}
