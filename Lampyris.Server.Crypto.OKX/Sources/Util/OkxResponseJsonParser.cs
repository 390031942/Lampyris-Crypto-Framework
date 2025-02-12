namespace Lampyris.Server.Crypto.Common;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public static class OkxResponseJsonParser
{
    public static List<QuoteTickerData> ParseTickerList(string json)
    {
        List<QuoteTickerData> result = new List<QuoteTickerData>();
        JObject jsonRoot = JsonUtil.TryPraseOkxResultJson(json);
        if (jsonRoot == null)
            return result;

        JArray data = jsonRoot["data"].ToObject<JArray>();
        foreach (JToken jToken in data)
        {
            JObject jObject = jToken.ToObject<JObject>();
            if (jObject != null)
            {
                QuoteTickerData quoteTickerData = new QuoteTickerData();
                string InstTypeStr = jObject["instType"].ToObject<string>();
                if (!Enum.TryParse(InstTypeStr, out InstType okxInstType))
                {
                    LogManager.Instance.LogError(
                        $"Error occurred while parsing json, reason: failed to parse Enum \"OkxInstType\"");
                    return result;
                }

                quoteTickerData.InstType = okxInstType;
                quoteTickerData.symbol = jObject["symbol"].ToObject<string>();
                quoteTickerData.Last = jObject["last"].ToObject<double>();
                quoteTickerData.LastSz = jObject["lastSz"].ToObject<double>();
                quoteTickerData.AskPx = jObject["askPx"].ToObject<double>();
                quoteTickerData.BidPx = jObject["bidPx"].ToObject<double>();
                quoteTickerData.BidSz = jObject["bidSz"].ToObject<double>();
                quoteTickerData.Open24h = jObject["open24h"].ToObject<double>();
                quoteTickerData.High24h = jObject["high24h"].ToObject<double>();
                quoteTickerData.Low24h = jObject["low24h"].ToObject<double>();
                quoteTickerData.VolCcy24h = jObject["volCcy24h"].ToObject<double>();
                quoteTickerData.Vol24h = jObject["vol24h"].ToObject<double>();
                quoteTickerData.SodUtc0 = jObject["sodUtc0"].ToObject<double>();
                quoteTickerData.SodUtc8 = jObject["sodUtc8"].ToObject<double>();
                quoteTickerData.Ts = jObject["ts"].ToObject<long>();

                result.Add(quoteTickerData);
            }
        }

        return result;
    }

    public static void ParseTickerListNoAlloc(string json, List<QuoteTickerData> result)
    {
        if (result == null)
            return;

        JObject jsonRoot = JsonUtil.TryPraseOkxResultJson(json);
        if (jsonRoot == null)
            return;

        JArray data = jsonRoot["data"].ToObject<JArray>();
        foreach (JToken jToken in data)
        {
            JObject jObject = jToken.ToObject<JObject>();
            if (jObject != null)
            {
                string InstTypeStr = jObject["instType"].ToObject<string>();
                if (!Enum.TryParse(InstTypeStr, out InstType okxInstType))
                {
                    LogManager.Instance.LogError(
                        $"Error occurred while parsing json, reason: failed to parse Enum \"OkxInstType\"");
                    return;
                }

                string symbol = jObject["symbol"].ToObject<string>();

                QuoteTickerData quoteTickerData = result.Find((QuoteTickerData data) => { return data.symbol == symbol; });
                if(quoteTickerData == null)
                {
                    quoteTickerData = new QuoteTickerData();
                    result.Add(quoteTickerData);
                }

                quoteTickerData.InstType = okxInstType;
                quoteTickerData.symbol = jObject["symbol"].ToObject<string>();
                quoteTickerData.Last = jObject["last"].ToObject<double>();
                quoteTickerData.LastSz = jObject["lastSz"].ToObject<double>();
                quoteTickerData.AskPx = jObject["askPx"].ToObject<double>();
                quoteTickerData.BidPx = jObject["bidPx"].ToObject<double>();
                quoteTickerData.BidSz = jObject["bidSz"].ToObject<double>();
                quoteTickerData.Open24h = jObject["open24h"].ToObject<double>();
                quoteTickerData.High24h = jObject["high24h"].ToObject<double>();
                quoteTickerData.Low24h = jObject["low24h"].ToObject<double>();
                quoteTickerData.VolCcy24h = jObject["volCcy24h"].ToObject<double>();
                quoteTickerData.Vol24h = jObject["vol24h"].ToObject<double>();
                quoteTickerData.SodUtc0 = jObject["sodUtc0"].ToObject<double>();
                quoteTickerData.SodUtc8 = jObject["sodUtc8"].ToObject<double>();
                quoteTickerData.Ts = jObject["ts"].ToObject<long>();
            }
        }
    }

    public static List<QuoteCandleData> ParseCandleList(string json)
    {
        List<QuoteCandleData> result = new List<QuoteCandleData>();
        ParseCandleListNoAlloc(json, result);
        return result;
    }

    public static void ParseCandleListNoAlloc(string json, List<QuoteCandleData> result)
    {
        JObject jsonRoot = JsonUtil.TryPraseOkxResultJson(json);
        if (jsonRoot == null)
            return;

        JArray data = jsonRoot["data"].ToObject<JArray>();

        foreach (JToken jToken in data)
        {
            JArray jArray = jToken.ToObject<JArray>();
            if (jArray != null && jArray.Count >= 9)
            {
                QuoteCandleData quoteCandleData = new QuoteCandleData();

                long timestamp = jArray[0].ToObject<long>();
                quoteCandleData.DateTime    = DateTimeUtil.FromUnixTimestamp(timestamp);
                quoteCandleData.Open        = jArray[1].ToObject<double>();
                quoteCandleData.High        = jArray[2].ToObject<double>();
                quoteCandleData.Low         = jArray[3].ToObject<double>();
                quoteCandleData.Close       = jArray[4].ToObject<double>();

                quoteCandleData.Volume         = jArray[5].ToObject<double>();
                quoteCandleData.Currency      = jArray[6].ToObject<double>();
                quoteCandleData.VolCcyQuote = jArray[7].ToObject<double>();

                quoteCandleData.DateTime    = quoteCandleData.DateTime.AddHours(8);
                result.Add(quoteCandleData);
            }
        }

        result.Reverse();
    }
}
