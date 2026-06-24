using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InvestmentApi.Services
{
    public class FinnhubService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly SecurityUpdateLogger _logger;
        private const string BaseUrl = "https://finnhub.io/api/v1";

        public FinnhubService(HttpClient httpClient, IConfiguration configuration, SecurityUpdateLogger logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Finnhub:ApiKey"] ?? "demo";
            _logger = logger;
        }

        public async Task<List<(string Symbol, string Name, string? Figi, string? Exchange, string Currency, string Type)>> GetStocksAsync(int limit = 5000)
        {
            var securities = new List<(string, string, string?, string?, string, string)>();
            
            try
            {
                _logger.Log("Pobieranie akcji z Finnhub...");
                
                // Poprawny endpoint: /stock/symbol (nie /stock/list)
                var url = $"{BaseUrl}/stock/symbol?exchange=US&token={_apiKey}";
                
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    var response = await _httpClient.GetAsync(url, cts.Token);
                    _logger.Log($"Status akcji: {response.StatusCode}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning($"Finnhub API zwróciło status: {response.StatusCode}");
                        return securities;
                    }
                    
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (content.TrimStart().StartsWith("<"))
                    {
                        _logger.LogWarning("Finnhub zwrócił HTML zamiast JSON");
                        return securities;
                    }
                    
                    var stocks = JArray.Parse(content);
                    _logger.Log($"Sparsowano {stocks.Count} papierów z Finnhub");
                    
                    var count = 0;
                    foreach (var stock in stocks.Take(limit))
                    {
                        try
                        {
                            var symbol = stock["symbol"]?.ToString();
                            var name = stock["description"]?.ToString();
                            var exchange = stock["mic"]?.ToString();
                            var currency = stock["currency"]?.ToString() ?? "USD";
                            var type = stock["type"]?.ToString() ?? "";
                            
                            // Finnhub nie dostarcza ISIN w /stock/symbol - zawsze pusty
                            // Zamiast tego użyjemy FIGI jako identyfikator
                            var isin = stock["figi"]?.ToString();
                            if (string.IsNullOrWhiteSpace(isin)) isin = null;
                            
                            if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(name))
                            {
                                // Odfiltruj ETF-y - zostaną pobrane osobno
                                if (!type.Contains("ETF", StringComparison.OrdinalIgnoreCase))
                                {
                                    securities.Add((symbol, name, isin, exchange, currency, "Stock"));
                                    count++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Błąd parsowania akcji: {ex.Message}");
                        }
                    }
                    
                    _logger.LogSuccess($"Pobrano {count} akcji");
                    return securities;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Pobieranie akcji: {ex.Message}");
                return securities;
            }
        }

        public async Task<List<(string Symbol, string Name, string? Figi, string? Exchange, string Currency, string Type)>> GetEtfsAsync(int limit = 2000)
        {
            var securities = new List<(string, string, string?, string?, string, string)>();
            
            try
            {
                _logger.Log("Pobieranie ETF-ów z Finnhub...");
                
                // Poprawny endpoint: /stock/symbol (nie /stock/list)
                var url = $"{BaseUrl}/stock/symbol?exchange=US&token={_apiKey}";
                
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    var response = await _httpClient.GetAsync(url, cts.Token);
                    _logger.Log($"Status ETF: {response.StatusCode}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Finnhub API zwróciło status: {response.StatusCode}");
                        return securities;
                    }
                    
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (content.TrimStart().StartsWith("<"))
                    {
                        _logger.LogWarning("Finnhub zwrócił HTML zamiast JSON");
                        return securities;
                    }
                    
                    var allSymbols = JArray.Parse(content);
                    
                    var count = 0;
                    foreach (var etf in allSymbols
                        .Where(e => e["type"]?.ToString()?.Contains("ETF", StringComparison.OrdinalIgnoreCase) == true)
                        .Take(limit))
                    {
                        try
                        {
                            var symbol = etf["symbol"]?.ToString();
                            var name = etf["description"]?.ToString();
                            var exchange = etf["mic"]?.ToString();
                            var currency = etf["currency"]?.ToString() ?? "USD";
                            
                            // Finnhub nie dostarcza ISIN w /stock/symbol - zawsze pusty
                            // Zamiast tego użyjemy FIGI jako identyfikator
                            var isin = etf["figi"]?.ToString();
                            if (string.IsNullOrWhiteSpace(isin)) isin = null;
                            
                            if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(name))
                            {
                                securities.Add((symbol, name, isin, exchange, currency, "ETF"));
                                count++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Błąd parsowania ETF: {ex.Message}");
                        }
                    }
                    
                    _logger.LogSuccess($"Pobrano {count} ETF-ów");
                    return securities;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Pobieranie ETF-ów: {ex.Message}");
                return securities;
            }
        }

        /// <summary>
        /// Pobiera aktualną cenę akcji/ETF z Finnhub
        /// </summary>
        public async Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{BaseUrl}/quote?symbol={symbol}&token={_apiKey}";
                
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(30));
                    
                    var response = await _httpClient.GetAsync(url, cts.Token);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Finnhub API zwróciło status {response.StatusCode} dla {symbol}");
                        return 0m;
                    }
                    
                    var content = await response.Content.ReadAsStringAsync(cts.Token);
                    var data = JObject.Parse(content);
                    
                    // Sprawdź czy jest cena
                    var price = data["c"]?.Value<decimal>();
                    
                    // Jeśli brak ceny, spróbuj z innym formatem (dla ETF-ów)
                    if (price == null || price == 0)
                    {
                        price = data["pc"]?.Value<decimal>(); // previous close
                    }
                    
                    if (price == null || price == 0)
                    {
                        _logger.LogWarning($"Brak ceny dla {symbol} w Finnhub");
                        return 0m;
                    }
                    
                    return price.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Błąd pobierania ceny dla {symbol}: {ex.Message}");
                return 0m;
            }
        }

        /// <summary>
        /// Pobiera historyczne ceny akcji/ETF z Finnhub
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetHistoricalPricesAsync(
            string symbol,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            var pricesByDate = new Dictionary<string, decimal>();
            
            try
            {
                // Finnhub zwraca dane dzienne (resolution=D)
                var from = new DateTimeOffset(fromDate.Date).ToUnixTimeSeconds();
                var to = new DateTimeOffset(toDate.Date.AddDays(1).AddTicks(-1)).ToUnixTimeSeconds();
                
                var url = $"{BaseUrl}/stock/candle?symbol={symbol}&resolution=D&from={from}&to={to}&token={_apiKey}";
                
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(30));
                    
                    var response = await _httpClient.GetAsync(url, cts.Token);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"Finnhub API zwróciło status {response.StatusCode} dla {symbol}");
                        return pricesByDate;
                    }
                    
                    var content = await response.Content.ReadAsStringAsync(cts.Token);
                    var data = JObject.Parse(content);
                    
                    var timestamps = data["t"]?.Values<long>().ToList() ?? new List<long>();
                    var closes = data["c"]?.Values<decimal>().ToList() ?? new List<decimal>();
                    
                    for (int i = 0; i < timestamps.Count && i < closes.Count; i++)
                    {
                        var date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]).DateTime.Date.ToString("yyyy-MM-dd");
                        var price = closes[i];
                        
                        if (price > 0 && !pricesByDate.ContainsKey(date))
                        {
                            pricesByDate[date] = price;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Błąd pobierania historii dla {symbol}: {ex.Message}");
            }
            
            return pricesByDate;
        }
    }
}
