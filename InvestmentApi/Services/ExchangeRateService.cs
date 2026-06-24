using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InvestmentApi.Services
{
    /// <summary>
    /// Serwis do pobierania kursów walut z NBP API
    /// </summary>
    public class ExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExchangeRateService> _logger;
        private const string NbpBaseUrl = "https://api.nbp.pl/api/exchangerates/rates/a";

        public ExchangeRateService(HttpClient httpClient, ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Pobiera kurs wymiany dla danej waluty na konkretny dzień
        /// </summary>
        /// <param name="currency">Kod waluty (USD, EUR, GBP itp.)</param>
        /// <param name="date">Data w formacie YYYY-MM-DD</param>
        /// <returns>Kurs wymiany lub null jeśli niedostępny</returns>
        public async Task<decimal?> GetExchangeRateAsync(string currency, DateTime date)
        {
            try
            {
                var dateStr = date.ToString("yyyy-MM-dd");
                var url = $"{NbpBaseUrl}/{currency.ToLower()}/{dateStr}/?format=json";

                _logger.LogInformation($"[ExchangeRate] Pobieranie kursu {currency} na dzień {dateStr}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(content);
                    var rate = data["rates"]?[0]?["mid"]?.Value<decimal>();

                    if (rate.HasValue && rate.Value > 0)
                    {
                        _logger.LogInformation($"[ExchangeRate] {currency}: {rate.Value:F4} PLN");
                        return rate.Value;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"[ExchangeRate] Brak kursu dla {currency} na dzień {dateStr} (weekend/święto)");
                    // Spróbuj znaleźć ostatni dostępny kurs przed tą datą
                    return await GetLastAvailableRateAsync(currency, date);
                }
                else
                {
                    _logger.LogWarning($"[ExchangeRate] NBP API zwróciło status {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ExchangeRate] Błąd pobierania kursu: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Pobiera ostatni dostępny kurs przed daną datą (maksymalnie 5 dni wstecz)
        /// </summary>
        private async Task<decimal?> GetLastAvailableRateAsync(string currency, DateTime date)
        {
            for (int daysBack = 1; daysBack <= 5; daysBack++)
            {
                var previousDate = date.AddDays(-daysBack);
                var dateStr = previousDate.ToString("yyyy-MM-dd");
                var url = $"{NbpBaseUrl}/{currency.ToLower()}/{dateStr}/?format=json";

                try
                {
                    var response = await _httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(content);
                        var rate = data["rates"]?[0]?["mid"]?.Value<decimal>();

                        if (rate.HasValue && rate.Value > 0)
                        {
                            _logger.LogInformation($"[ExchangeRate] {currency}: Użyto kursu z {dateStr}: {rate.Value:F4} PLN");
                            return rate.Value;
                        }
                    }
                }
                catch
                {
                    // Kontynuuj szukanie
                }
            }

            _logger.LogWarning($"[ExchangeRate] Nie znaleziono kursu dla {currency} w ciągu ostatnich 5 dni");
            return null;
        }

        /// <summary>
        /// Przelicza cenę z waluty obcej na PLN
        /// </summary>
        public decimal ConvertToPln(decimal price, decimal exchangeRate)
        {
            return price * exchangeRate;
        }
    }
}
