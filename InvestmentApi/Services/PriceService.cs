using System.Net.Http.Json;
using InvestmentApi.Data;
using InvestmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentApi.Services
{
    public class PriceService : IPriceService
    {
        private readonly InvestmentContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PriceService> _logger;
        private readonly FinnhubService _finnhubService;
        private readonly SecurityUpdateLogger _securityLogger;

        // Cache TTL: 5 minut dla krypto, 24h dla akcji/ETF
        private static readonly TimeSpan CryptoCacheTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan StockCacheTtl = TimeSpan.FromHours(24);

        public PriceService(
            InvestmentContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<PriceService> logger,
            FinnhubService finnhubService,
            SecurityUpdateLogger securityLogger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _finnhubService = finnhubService;
            _securityLogger = securityLogger;
        }

        public async Task<Dictionary<string, decimal>> GetCurrentPricesAsync(
            IEnumerable<string> symbols,
            CancellationToken cancellationToken = default)
        {
            var normalizedSymbols = symbols
                .Select(AssetSymbolMapper.NormalizeSymbol)
                .Distinct()
                .ToArray();

            if (normalizedSymbols.Length == 0)
                return new Dictionary<string, decimal>();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var cryptoToFetch = new List<string>();
            var stocksToFetch = new List<string>();

            foreach (var symbol in normalizedSymbols)
            {
                var security = await _context.Securities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Symbol == symbol, cancellationToken);

                var isCrypto = security?.Type == "Crypto";
                var cacheTtl = isCrypto ? CryptoCacheTtl : StockCacheTtl;

                var asset = await _context.Assets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Symbol == symbol, cancellationToken);

                if (asset != null)
                {
                    var cached = await _context.PriceSnapshots
                        .AsNoTracking()
                        .Where(p => p.AssetId == asset.Id && p.SnapshotDate == today)
                        .OrderByDescending(p => p.FetchedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (cached != null && DateTime.UtcNow - cached.FetchedAt < cacheTtl)
                    {
                        result[symbol] = cached.PricePln;
                        continue;
                    }
                }

                if (isCrypto)
                    cryptoToFetch.Add(symbol);
                else
                    stocksToFetch.Add(symbol);
            }

            // Pobierz ceny kryptowalut z CoinGecko
            if (cryptoToFetch.Count > 0)
            {
                var cryptoPrices = await FetchCryptoPricesAsync(cryptoToFetch, cancellationToken);
                foreach (var (symbol, price) in cryptoPrices)
                {
                    result[symbol] = price;
                    await UpsertTodaySnapshotAsync(symbol, price, cancellationToken);
                }
            }

            // Pobierz ceny akcji/ETF z Finnhub (tylko te z portfela)
            if (stocksToFetch.Count > 0)
            {
                var stockPrices = await FetchStockPricesWithRetryAsync(stocksToFetch, cancellationToken);
                foreach (var (symbol, price) in stockPrices)
                {
                    result[symbol] = price;
                    await UpsertTodaySnapshotAsync(symbol, price, cancellationToken);
                }
            }

            return result;
        }

        /// <summary>
        /// Pobiera ceny akcji/ETF z Finnhub dla instrumentów w portfelu użytkownika
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetCurrentPricesForPortfolioAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            // Pobierz tylko symbole które użytkownik ma w portfelu
            var portfolioSymbols = await _context.Transactions
                .AsNoTracking()
                .Where(t => t.Asset.Transactions.Any(tx => tx.AssetId == t.AssetId))
                .Select(t => t.Asset.Symbol)
                .Distinct()
                .ToListAsync(cancellationToken);

            return await GetCurrentPricesAsync(portfolioSymbols, cancellationToken);
        }

        public async Task<Dictionary<string, Dictionary<string, decimal>>> GetHistoricalPricesAsync(
            IEnumerable<string> symbols,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            var normalizedSymbols = symbols
                .Select(AssetSymbolMapper.NormalizeSymbol)
                .Distinct()
                .ToArray();

            if (normalizedSymbols.Length == 0)
                return new Dictionary<string, Dictionary<string, decimal>>();

            var from = DateOnly.FromDateTime(fromDate.Date);
            var to = DateOnly.FromDateTime(toDate.Date);
            var result = new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);

            foreach (var symbol in normalizedSymbols)
            {
                var asset = await EnsureAssetAsync(symbol, cancellationToken);
                var security = await _context.Securities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Symbol == symbol, cancellationToken);

                var cachedPrices = await _context.PriceSnapshots
                    .AsNoTracking()
                    .Where(p => p.AssetId == asset.Id && p.SnapshotDate >= from && p.SnapshotDate <= to)
                    .ToListAsync(cancellationToken);

                var pricesByDate = cachedPrices.ToDictionary(
                    p => p.SnapshotDate.ToString("yyyy-MM-dd"),
                    p => p.PricePln);

                var missingRanges = FindMissingDateRanges(from, to, cachedPrices.Select(p => p.SnapshotDate).ToHashSet());

                foreach (var (rangeStart, rangeEnd) in missingRanges)
                {
                    Dictionary<string, decimal> fetched;

                    if (security?.Type == "Crypto")
                    {
                        var coinGeckoId = AssetSymbolMapper.ToCoinGeckoId(symbol);
                        fetched = await FetchHistoricalRangeFromApiAsync(
                            coinGeckoId,
                            rangeStart.ToDateTime(TimeOnly.MinValue),
                            rangeEnd.ToDateTime(TimeOnly.MinValue),
                            cancellationToken);
                    }
                    else
                    {
                        // Dla akcji/ETF pobierz z Finnhub z retry logic
                        fetched = await FetchHistoricalStockPricesWithRetryAsync(
                            symbol,
                            rangeStart.ToDateTime(TimeOnly.MinValue),
                            rangeEnd.ToDateTime(TimeOnly.MinValue),
                            cancellationToken);
                    }

                    foreach (var (date, price) in fetched)
                    {
                        if (!pricesByDate.ContainsKey(date))
                            pricesByDate[date] = price;
                    }

                    await UpsertHistoricalSnapshotsAsync(asset.Id, fetched, cancellationToken);
                }

                result[symbol] = pricesByDate;
            }

            return result;
        }

        private async Task<Dictionary<string, decimal>> FetchCryptoPricesAsync(
            List<string> symbols,
            CancellationToken cancellationToken)
        {
            var ids = symbols
                .Select(AssetSymbolMapper.ToCoinGeckoId)
                .Distinct()
                .ToArray();

            var client = CreateClient();
            var url = $"https://api.coingecko.com/api/v3/simple/price?ids={string.Join(',', ids)}&vs_currencies=pln";

            try
            {
                var response = await client.GetFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>(url, cancellationToken);
                var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                foreach (var symbol in symbols)
                {
                    var id = AssetSymbolMapper.ToCoinGeckoId(symbol);
                    result[symbol] = response != null && response.TryGetValue(id, out var price) && price.TryGetValue("pln", out var pln)
                        ? pln
                        : 0m;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nie udało się pobrać aktualnych cen z CoinGecko");
                return symbols.ToDictionary(s => s, _ => 0m, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Pobiera ceny akcji/ETF z Finnhub z retry logic (429 → czekaj 60s, max 3 próby)
        /// </summary>
        private async Task<Dictionary<string, decimal>> FetchStockPricesWithRetryAsync(
            List<string> symbols,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var symbol in symbols)
            {
                decimal price = 0m;
                int maxRetries = 3;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        price = await _finnhubService.GetCurrentPriceAsync(symbol, cancellationToken);

                        if (price > 0)
                        {
                            _securityLogger.Log($"[PRICE] {symbol}: {price:F2} PLN (próba {attempt})");
                            break;
                        }
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _securityLogger.LogWarning($"[PRICE] {symbol}: 429 Too Many Requests - czekam 60s (próba {attempt}/{maxRetries})");
                        if (attempt < maxRetries)
                            await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _securityLogger.LogWarning($"[PRICE] {symbol}: Błąd - {ex.Message} (próba {attempt}/{maxRetries})");
                        if (attempt < maxRetries)
                            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    }
                }

                // Jeśli nie udało się pobrać z Finnhub, użyj ostatniej zapisanej ceny
                if (price <= 0)
                {
                    var asset = await _context.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Symbol == symbol, cancellationToken);
                    if (asset != null)
                    {
                        var lastPrice = await _context.PriceSnapshots
                            .AsNoTracking()
                            .Where(p => p.AssetId == asset.Id && p.PricePln > 0)
                            .OrderByDescending(p => p.SnapshotDate)
                            .ThenByDescending(p => p.FetchedAt)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (lastPrice != null)
                        {
                            price = lastPrice.PricePln;
                            _securityLogger.Log($"[PRICE] {symbol}: Używam ostatniej zapisanej ceny {price:F2} PLN z {lastPrice.SnapshotDate}");
                        }
                        else
                        {
                            // Jeśli brak ostatniej ceny, użyj ceny z ostatniej transakcji
                            var lastTransaction = await _context.Transactions
                                .AsNoTracking()
                                .Where(t => t.AssetId == asset.Id)
                                .OrderByDescending(t => t.TransactionDate)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (lastTransaction != null && lastTransaction.Price > 0)
                            {
                                price = lastTransaction.Price;
                                _securityLogger.Log($"[PRICE] {symbol}: Używam ceny z ostatniej transakcji {price:F2} PLN");
                            }
                        }
                    }
                }

                result[symbol] = price;

                // Opóźnienie 1 sekundy między zapytaniami (limit Finnhub)
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            return result;
        }

        private async Task<Dictionary<string, decimal>> FetchHistoricalRangeFromApiAsync(
            string coinGeckoId,
            DateTime rangeStart,
            DateTime rangeEnd,
            CancellationToken cancellationToken)
        {
            var pricesByDate = new Dictionary<string, decimal>();
            var chunkStart = rangeStart.Date;

            while (chunkStart <= rangeEnd.Date)
            {
                var chunkEnd = chunkStart.AddDays(89);
                if (chunkEnd > rangeEnd.Date)
                    chunkEnd = rangeEnd.Date;

                try
                {
                    var from = new DateTimeOffset(chunkStart).ToUnixTimeSeconds();
                    var toUnix = new DateTimeOffset(chunkEnd.AddDays(1).AddTicks(-1)).ToUnixTimeSeconds();
                    var url = $"https://api.coingecko.com/api/v3/coins/{coinGeckoId}/market_chart/range?vs_currency=pln&from={from}&to={toUnix}";

                    var client = CreateClient();
                    var response = await client.GetFromJsonAsync<MarketChartResponse>(url, cancellationToken);

                    if (response?.Prices != null)
                    {
                        foreach (var point in response.Prices.Where(p => p.Count >= 2))
                        {
                            var date = TimeZoneInfo.ConvertTimeFromUtc(
                                DateTimeOffset.FromUnixTimeMilliseconds((long)point[0]).UtcDateTime,
                                TimeZoneInfo.Local).Date.ToString("yyyy-MM-dd");
                            var price = (decimal)point[1];

                            if (!pricesByDate.ContainsKey(date))
                                pricesByDate[date] = price;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Nie udało się pobrać historii dla {CoinGeckoId} ({Start} - {End})",
                        coinGeckoId, chunkStart, chunkEnd);
                }

                chunkStart = chunkEnd.AddDays(1);
                await Task.Delay(1200, cancellationToken);
            }

            return pricesByDate;
        }

        /// <summary>
        /// Pobiera historyczne ceny akcji/ETF z Finnhub z retry logic
        /// </summary>
        private async Task<Dictionary<string, decimal>> FetchHistoricalStockPricesWithRetryAsync(
            string symbol,
            DateTime rangeStart,
            DateTime rangeEnd,
            CancellationToken cancellationToken)
        {
            int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var prices = await _finnhubService.GetHistoricalPricesAsync(symbol, rangeStart, rangeEnd, cancellationToken);

                    if (prices.Count > 0)
                    {
                        _securityLogger.Log($"[HISTORY] {symbol}: Pobrano {prices.Count} cen historycznych (próba {attempt})");
                        return prices;
                    }

                    _securityLogger.LogWarning($"[HISTORY] {symbol}: Brak danych historycznych (próba {attempt}/{maxRetries})");
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _securityLogger.LogWarning($"[HISTORY] {symbol}: 429 Too Many Requests - czekam 60s (próba {attempt}/{maxRetries})");
                    if (attempt < maxRetries)
                        await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                }
                catch (Exception ex)
                {
                    _securityLogger.LogWarning($"[HISTORY] {symbol}: Błąd - {ex.Message} (próba {attempt}/{maxRetries})");
                    if (attempt < maxRetries)
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }

            return new Dictionary<string, decimal>();
        }

        private async Task UpsertTodaySnapshotAsync(string symbol, decimal price, CancellationToken cancellationToken)
        {
            if (price <= 0)
                return;

            var asset = await EnsureAssetAsync(symbol, cancellationToken);
            var today = DateOnly.FromDateTime(DateTime.Today);

            var existing = await _context.PriceSnapshots
                .FirstOrDefaultAsync(p => p.AssetId == asset.Id && p.SnapshotDate == today, cancellationToken);

            if (existing == null)
            {
                _context.PriceSnapshots.Add(new PriceSnapshot
                {
                    AssetId = asset.Id,
                    SnapshotDate = today,
                    PricePln = price,
                    FetchedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.PricePln = price;
                existing.FetchedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertHistoricalSnapshotsAsync(
            int assetId,
            Dictionary<string, decimal> prices,
            CancellationToken cancellationToken)
        {
            if (prices.Count == 0)
                return;

            var dates = prices.Keys.Select(d => DateOnly.Parse(d)).ToArray();

            var existing = await _context.PriceSnapshots
                .Where(p => p.AssetId == assetId && dates.Contains(p.SnapshotDate))
                .ToListAsync(cancellationToken);

            var existingByDate = existing.ToDictionary(p => p.SnapshotDate);

            foreach (var (dateString, price) in prices)
            {
                if (price <= 0)
                    continue;

                var date = DateOnly.Parse(dateString);

                if (existingByDate.TryGetValue(date, out var snapshot))
                {
                    snapshot.PricePln = price;
                    snapshot.FetchedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.PriceSnapshots.Add(new PriceSnapshot
                    {
                        AssetId = assetId,
                        SnapshotDate = date,
                        PricePln = price,
                        FetchedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Asset> EnsureAssetAsync(string symbol, CancellationToken cancellationToken)
        {
            var normalized = AssetSymbolMapper.NormalizeSymbol(symbol);
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Symbol == normalized, cancellationToken);

            if (asset != null)
                return asset;

            asset = new Asset
            {
                Symbol = normalized,
                CoinGeckoId = AssetSymbolMapper.ToCoinGeckoId(normalized),
                Name = AssetSymbolMapper.GetDisplayName(normalized),
                IsActive = true
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync(cancellationToken);
            return asset;
        }

        private static List<(DateOnly Start, DateOnly End)> FindMissingDateRanges(
            DateOnly from,
            DateOnly to,
            HashSet<DateOnly> cachedDates)
        {
            var ranges = new List<(DateOnly Start, DateOnly End)>();
            DateOnly? rangeStart = null;

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                if (cachedDates.Contains(day))
                {
                    if (rangeStart != null)
                    {
                        ranges.Add((rangeStart.Value, day.AddDays(-1)));
                        rangeStart = null;
                    }
                }
                else
                {
                    rangeStart ??= day;
                }
            }

            if (rangeStart != null)
                ranges.Add((rangeStart.Value, to));

            return ranges;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("InvestmentApp/1.0");
            return client;
        }

        private class MarketChartResponse
        {
            public List<List<double>> Prices { get; set; } = [];
        }
    }
}
