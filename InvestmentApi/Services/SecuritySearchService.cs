using InvestmentApi.Data;
using InvestmentApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InvestmentApi.Services
{
    public interface ISecuritySearchService
    {
        Task<List<SecuritySearchResult>> SearchSecuritiesAsync(string query, int limit = 20, CancellationToken cancellationToken = default);
    }

    public class SecuritySearchService : ISecuritySearchService
    {
        private readonly InvestmentContext _context;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY_PREFIX = "security_search_";
        private const int CACHE_DURATION_MINUTES = 60;

        public SecuritySearchService(InvestmentContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<SecuritySearchResult>> SearchSecuritiesAsync(
            string query, 
            int limit = 20, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<SecuritySearchResult>();

            var normalizedQuery = query.Trim().ToUpperInvariant();
            var cacheKey = $"{CACHE_KEY_PREFIX}{normalizedQuery}_{limit}";

            // Sprawdź cache
            if (_cache.TryGetValue(cacheKey, out List<SecuritySearchResult>? cachedResults))
            {
                return cachedResults ?? new List<SecuritySearchResult>();
            }

            // Wyszukaj w bazie
            var results = await _context.Securities
                .Where(s => s.IsActive)
                .Where(s => 
                    s.Symbol.ToUpper().Contains(normalizedQuery) ||
                    s.Name.ToUpper().Contains(normalizedQuery))
                .OrderBy(s => 
                    s.Symbol.ToUpper() == normalizedQuery ? 0 :  // Exact match
                    s.Symbol.ToUpper().StartsWith(normalizedQuery) ? 1 :  // Starts with
                    2)  // Contains
                .ThenBy(s => s.Symbol)
                .Take(limit)
                .Select(s => new SecuritySearchResult
                {
                    Id = s.Id,
                    Symbol = s.Symbol,
                    Name = s.Name,
                    Type = s.Type,
                    Exchange = s.Exchange,
                    Currency = s.Currency,
                    CoinGeckoId = s.CoinGeckoId
                })
                .ToListAsync(cancellationToken);

            // Zapisz do cache
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
            _cache.Set(cacheKey, results, cacheOptions);

            return results;
        }
    }
}
