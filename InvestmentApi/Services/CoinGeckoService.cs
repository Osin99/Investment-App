using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InvestmentApi.Services
{
    public class CoinGeckoService
    {
        private readonly HttpClient _httpClient;
        private readonly SecurityUpdateLogger _logger;
        private const string BaseUrl = "https://api.coingecko.com/api/v3";

        public CoinGeckoService(HttpClient httpClient, SecurityUpdateLogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Dodaj User-Agent header - CoinGecko wymaga go
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "InvestmentApp/1.0");
            }
        }

        public async Task<List<(string Symbol, string Name, string? Isin, string? Exchange, string Currency, string Type)>> GetCryptocurrenciesAsync(int limit = 500)
        {
            var securities = new List<(string, string, string?, string?, string, string)>();
            
            try
            {
                _logger.Log("Pobieranie kryptowalut z CoinGecko...");
                
                // Endpoint /coins/list zwraca WSZYSTKIE kryptowaluty (bez paginacji)
                // Nie obsługuje order=market_cap_desc - to jest endpoint /coins/markets
                var url = $"{BaseUrl}/coins/list?include_platform=false";
                
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    var response = await _httpClient.GetAsync(url, cts.Token);
                    _logger.Log($"Status CoinGecko: {response.StatusCode}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning($"CoinGecko API zwróciło status: {response.StatusCode}");
                        
                        // Spróbuj alternatywny endpoint
                        return await GetCryptocurrenciesFromMarketsAsync(limit);
                    }
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var cryptos = JArray.Parse(content);
                    _logger.Log($"Sparsowano {cryptos.Count} kryptowalut z CoinGecko");
                    
                    var count = 0;
                    foreach (var crypto in cryptos.Take(limit))
                    {
                        try
                        {
                            var symbol = crypto["symbol"]?.ToString()?.ToUpperInvariant();
                            var name = crypto["name"]?.ToString();
                            var id = crypto["id"]?.ToString();
                            
                            if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(name))
                            {
                                securities.Add((symbol, name, null, null, "USD", "Crypto"));
                                count++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Błąd parsowania kryptowaluty: {ex.Message}");
                        }
                    }
                    
                    _logger.LogSuccess($"Pobrano {count} kryptowalut");
                    return securities;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Pobieranie kryptowalut: {ex.Message}");
                return securities;
            }
        }

        private async Task<List<(string Symbol, string Name, string? Isin, string? Exchange, string Currency, string Type)>> GetCryptocurrenciesFromMarketsAsync(int limit = 500)
        {
            var securities = new List<(string, string, string?, string?, string, string)>();
            
            try
            {
                _logger.Log("Próba alternatywnego endpointu CoinGecko /coins/markets...");
                
                // Pobierz top kryptowaluty według market cap
                var url = $"{BaseUrl}/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=250&page=1&sparkline=false";
                
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    var response = await _httpClient.GetAsync(url, cts.Token);
                    _logger.Log($"Status CoinGecko markets: {response.StatusCode}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning($"CoinGecko markets API zwróciło status: {response.StatusCode}");
                        return securities;
                    }
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var cryptos = JArray.Parse(content);
                    
                    var count = 0;
                    foreach (var crypto in cryptos.Take(limit))
                    {
                        try
                        {
                            var symbol = crypto["symbol"]?.ToString()?.ToUpperInvariant();
                            var name = crypto["name"]?.ToString();
                            
                            if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(name))
                            {
                                securities.Add((symbol, name, null, null, "USD", "Crypto"));
                                count++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Błąd parsowania kryptowaluty: {ex.Message}");
                        }
                    }
                    
                    _logger.LogSuccess($"Pobrano {count} kryptowalut (markets)");
                    return securities;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Pobieranie kryptowalut (markets): {ex.Message}");
                return securities;
            }
        }
    }
}
