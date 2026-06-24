using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentApi.Data;
using InvestmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentApi.Services
{
    public class SecurityUpdateService
    {
        private readonly InvestmentContext _context;
        private readonly FinnhubService _finnhubService;
        private readonly CoinGeckoService _coinGeckoService;
        private readonly SecurityUpdateLogger _logger;

        public SecurityUpdateService(
            InvestmentContext context,
            FinnhubService finnhubService,
            CoinGeckoService coinGeckoService,
            SecurityUpdateLogger logger)
        {
            _context = context;
            _finnhubService = finnhubService;
            _coinGeckoService = coinGeckoService;
            _logger = logger;
        }

        public async Task UpdateSecuritiesAsync()
        {
            var startTime = DateTime.UtcNow;
            _logger.LogSection("SECURITY UPDATE");
            _logger.Log($"Data: {startTime:yyyy-MM-dd HH:mm:ss} UTC");
            _logger.Log("Status: STARTED");

            try
            {
                var allSecurities = new List<(string Symbol, string Name, string? Isin, string? Exchange, string Currency, string Type)>();
                int totalUpdated = 0;
                int totalAdded = 0;

                // Pobierz akcje
                _logger.LogSection("AKCJE");
                var stocks = await _finnhubService.GetStocksAsync(5000);
                allSecurities.AddRange(stocks);
                var (stocksUpdated, stocksAdded) = await UpsertSecuritiesAsync(stocks, "Finnhub");
                totalUpdated += stocksUpdated;
                totalAdded += stocksAdded;

                // Pobierz ETF-y
                _logger.LogSection("ETF-Y");
                var etfs = await _finnhubService.GetEtfsAsync(2000);
                allSecurities.AddRange(etfs);
                var (etfsUpdated, etfsAdded) = await UpsertSecuritiesAsync(etfs, "Finnhub");
                totalUpdated += etfsUpdated;
                totalAdded += etfsAdded;

                // Pobierz kryptowaluty
                _logger.LogSection("KRYPTOWALUTY");
                var cryptos = await _coinGeckoService.GetCryptocurrenciesAsync(500);
                allSecurities.AddRange(cryptos);
                var (cryptosUpdated, cryptosAdded) = await UpsertSecuritiesAsync(cryptos, "CoinGecko");
                totalUpdated += cryptosUpdated;
                totalAdded += cryptosAdded;

                var duration = DateTime.UtcNow - startTime;
                _logger.LogSummary(allSecurities.Count, totalUpdated, totalAdded, duration, "SUCCESS ✓");
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError($"Główny błąd: {ex.Message}");
                _logger.LogSummary(0, 0, 0, duration, "FAILED ❌");
                throw;
            }
        }

        private async Task<(int Updated, int Added)> UpsertSecuritiesAsync(
            List<(string Symbol, string Name, string? Figi, string? Exchange, string Currency, string Type)> securities,
            string dataSource)
        {
            int updated = 0;
            int added = 0;

            // Pobierz wszystkie istniejące symbole jednym zapytaniem
            var allSymbols = securities.Select(s => s.Symbol).ToHashSet();
            var existingSecurities = await _context.Securities
                .Where(s => allSymbols.Contains(s.Symbol))
                .ToDictionaryAsync(s => s.Symbol);

            var batchSize = 100;
            var batch = new List<Security>();

            foreach (var security in securities)
            {
                try
                {
                    // Walidacja typu - musi być jednym z: "Stock", "ETF", "Crypto", "Other"
                    var validTypes = new[] { "Stock", "ETF", "Crypto", "Other" };
                    if (!validTypes.Contains(security.Type))
                    {
                        _logger.LogWarning($"Nieznany typ: {security.Type} dla {security.Symbol}");
                        continue;
                    }

                    if (existingSecurities.TryGetValue(security.Symbol, out var existing))
                    {
                        // Update
                        existing.Name = security.Name;
                        existing.Type = security.Type;  // Teraz Type jest string
                        existing.Figi = security.Figi;  // Używamy FIGI zamiast ISIN
                        existing.Exchange = security.Exchange;
                        existing.Currency = security.Currency;
                        existing.LastUpdated = DateTime.UtcNow;
                        existing.DataSource = dataSource;
                        existing.IsActive = true;
                        updated++;
                    }
                    else
                    {
                        // Insert
                        var newSecurity = new Security
                        {
                            Symbol = security.Symbol,
                            Name = security.Name,
                            Type = security.Type,  // Teraz Type jest string
                            Figi = security.Figi,  // Używamy FIGI zamiast ISIN
                            Exchange = security.Exchange,
                            Currency = security.Currency,
                            LastUpdated = DateTime.UtcNow,
                            DataSource = dataSource,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _context.Securities.Add(newSecurity);
                        // Dodaj do słownika żeby uniknąć duplikatów w tej samej partii
                        existingSecurities[security.Symbol] = newSecurity;
                        added++;
                    }

                    // Zapisuj co 100 rekordów
                    if ((updated + added) % batchSize == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Błąd upsert {security.Symbol}: {ex.Message}");
                }
            }

            // Zapisz pozostałe
            await _context.SaveChangesAsync();
            _logger.LogSuccess($"Zaktualizowano {updated}, dodano {added}");
            
            return (updated, added);
        }
    }
}
