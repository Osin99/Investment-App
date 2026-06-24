using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvestmentApi.Services;
using InvestmentApi.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly SecurityUpdateService _securityUpdateService;
        private readonly SecurityUpdateLogger _logger;
        private readonly IPriceService _priceService;
        private readonly InvestmentContext _context;

        // Cooldown: 1 godzina między ręcznymi aktualizacjami cen
        private static DateTime? _lastPriceUpdateTime = null;
        private static readonly TimeSpan PriceUpdateCooldown = TimeSpan.FromHours(1);

        public AdminController(
            SecurityUpdateService securityUpdateService,
            SecurityUpdateLogger logger,
            IPriceService priceService,
            InvestmentContext context)
        {
            _securityUpdateService = securityUpdateService;
            _logger = logger;
            _priceService = priceService;
            _context = context;
        }

        [HttpPost("update-securities")]
        public async Task<IActionResult> UpdateSecurities()
        {
            try
            {
                _logger.Log("Ręczne pobieranie papierów wartościowych zainicjowane przez użytkownika");
                await _securityUpdateService.UpdateSecuritiesAsync();
                return Ok(new { message = "Papierów wartościowych zostały zaktualizowane pomyślnie" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas ręcznego pobierania: {ex.Message}");
                return StatusCode(500, new { error = "Błąd podczas aktualizacji papierów wartościowych", details = ex.Message });
            }
        }

        [HttpPost("update-prices")]
        public async Task<IActionResult> UpdatePrices()
        {
            // Sprawdź cooldown
            if (_lastPriceUpdateTime.HasValue)
            {
                var elapsed = DateTime.UtcNow - _lastPriceUpdateTime.Value;
                if (elapsed < PriceUpdateCooldown)
                {
                    var nextAvailable = _lastPriceUpdateTime.Value.Add(PriceUpdateCooldown);
                    return BadRequest(new
                    {
                        error = "Cooldown aktywny",
                        message = $"Następna aktualizacja możliwa o {nextAvailable.ToLocalTime():HH:mm}",
                        nextAvailableAt = nextAvailable.ToLocalTime().ToString("HH:mm"),
                        remainingSeconds = (int)(PriceUpdateCooldown - elapsed).TotalSeconds
                    });
                }
            }

            try
            {
                _logger.Log("Ręczna aktualizacja cen zainicjowana przez użytkownika");
                _lastPriceUpdateTime = DateTime.UtcNow;

                // Pobierz symbole akcji/ETF z portfela
                var stockSymbols = await _context.Transactions
                    .AsNoTracking()
                    .Select(t => t.Asset.Symbol)
                    .Distinct()
                    .ToListAsync();

                var filteredSymbols = new List<string>();
                foreach (var symbol in stockSymbols)
                {
                    var security = await _context.Securities
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Symbol == symbol);

                    if (security?.Type == "Stock" || security?.Type == "ETF")
                        filteredSymbols.Add(symbol);
                }

                if (filteredSymbols.Count > 0)
                {
                    var prices = await _priceService.GetCurrentPricesAsync(filteredSymbols);
                    var updated = prices.Count(p => p.Value > 0);
                    _logger.Log($"Ręczna aktualizacja cen: zaktualizowano {updated}/{filteredSymbols.Count} instrumentów");

                    return Ok(new
                    {
                        message = $"Zaktualizowano ceny dla {updated} z {filteredSymbols.Count} instrumentów",
                        updatedCount = updated,
                        totalCount = filteredSymbols.Count,
                        updatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        status = "SUCCESS"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Brak akcji/ETF w portfelu",
                        updatedCount = 0,
                        totalCount = 0,
                        updatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        status = "SUCCESS"
                    });
                }
            }
            catch (Exception ex)
            {
                _lastPriceUpdateTime = null; // Reset cooldown przy błędzie
                _logger.LogError($"Błąd podczas ręcznej aktualizacji cen: {ex.Message}");
                return StatusCode(500, new { error = "Błąd podczas aktualizacji cen", details = ex.Message, status = "FAILED" });
            }
        }

        [HttpGet("price-update-status")]
        public IActionResult GetPriceUpdateStatus()
        {
            if (_lastPriceUpdateTime.HasValue)
            {
                var elapsed = DateTime.UtcNow - _lastPriceUpdateTime.Value;
                var isOnCooldown = elapsed < PriceUpdateCooldown;
                var nextAvailable = _lastPriceUpdateTime.Value.Add(PriceUpdateCooldown);

                return Ok(new
                {
                    lastUpdatedAt = _lastPriceUpdateTime.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    isOnCooldown,
                    nextAvailableAt = isOnCooldown ? nextAvailable.ToLocalTime().ToString("HH:mm") : null,
                    remainingSeconds = isOnCooldown ? (int)(PriceUpdateCooldown - elapsed).TotalSeconds : 0
                });
            }

            return Ok(new
            {
                lastUpdatedAt = (string?)null,
                isOnCooldown = false,
                nextAvailableAt = (string?)null,
                remainingSeconds = 0
            });
        }
    }
}
