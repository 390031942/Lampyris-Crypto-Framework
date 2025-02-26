using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class BinanceApiService
{
    private const string ExchangeInfoUrl = "https://fapi.binance.com/fapi/v1/exchangeInfo";
    private readonly HttpClient _httpClient;

    public BinanceApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<SymbolInfo>> GetUsdtPerpetualSymbolsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(ExchangeInfoUrl);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        var exchangeInfo = JsonSerializer.Deserialize<ExchangeInfo>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var symbols = new List<SymbolInfo>();
        foreach (var symbol in exchangeInfo.Symbols)
        {
            if (symbol.QuoteAsset == "USDT" && symbol.ContractType == "PERPETUAL")
            {
                symbols.Add(new SymbolInfo
                {
                    Symbol = symbol.Symbol,
                    OnboardDate = symbol.OnboardDate
                });
            }
        }

        return symbols;
    }
}