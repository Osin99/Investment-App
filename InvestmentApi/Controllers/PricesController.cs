using Microsoft.AspNetCore.Mvc;
using InvestmentApi.Services;

namespace InvestmentApi.Controllers
{
    [ApiController]
    [Route("api/prices")]
    public class PricesController : ControllerBase
    {
        private readonly IPriceService _priceService;
        private readonly ILogger<PricesController> _logger;

        public PricesController(IPriceService priceService, ILogger<PricesController> logger)
        {
            _priceService = priceService;
            _logger = logger;
        }

        /// <summary>
        /// Pobiera aktualne ceny (kryptowalut, akcji, ETF)
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentPrices(
            [FromQuery] string symbols = "BTC,ETH,SOL",
            [FromQuery] string type = "crypto")
        {
            try
            {
                var symbolList = symbols.Split(',').Select(s => s.Trim().ToUpper()).ToList();
                
                if (symbolList.Count == 0)
                    return BadRequest(new { message = "Brak symboli do pobrania" });

                Dictionary<string, decimal> prices = new();

                // Pobierz ceny w zależności od typu
                if (type.ToLower() == "stocks")
                {
                    // Pobierz ceny akcji/ETF z Finnhub
                    prices = await _priceService.GetCurrentPricesAsync(symbolList);
                }
                else if (type.ToLower() == "all")
                {
                    // Pobierz ceny dla wszystkich typów
                    prices = await _priceService.GetCurrentPricesAsync(symbolList);
                }
                else
                {
                    // Domyślnie pobierz ceny kryptowalut
                    prices = await _priceService.GetCurrentPricesAsync(symbolList);
                }

                return Ok(prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd pobierania cen");
                return StatusCode(500, new { message = "Błąd pobierania cen" });
            }
        }

        /// <summary>
        /// Pobiera ceny akcji i ETF
        /// </summary>
        [HttpGet("stocks")]
        public async Task<IActionResult> GetStockPrices([FromQuery] string symbols = "AAPL,MSFT")
        {
            try
            {
                var symbolList = symbols.Split(',').Select(s => s.Trim().ToUpper()).ToList();
                var prices = await _priceService.GetCurrentPricesAsync(symbolList);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd pobierania cen akcji");
                return StatusCode(500, new { message = "Błąd pobierania cen akcji" });
            }
        }

        /// <summary>
        /// Pobiera ceny złota
        /// </summary>
        [HttpGet("gold")]
        public IActionResult GetGoldPrices([FromQuery] string units = "g,oz")
        {
            try
            {
                var unitList = units.Split(',').Select(s => s.Trim().ToLower()).ToList();
                
                // TODO: Integracja z API cen złota (np. Metals API, Yahoo Finance)
                // Na razie zwracamy placeholder
                var prices = new Dictionary<string, decimal>();
                
                foreach (var unit in unitList)
                {
                    if (unit == "g")
                        prices["XAU_g"] = 250m; // Placeholder: cena za gram
                    else if (unit == "oz")
                        prices["XAU_oz"] = 2000m; // Placeholder: cena za uncję
                }

                return Ok(prices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd pobierania cen złota");
                return StatusCode(500, new { message = "Błąd pobierania cen złota" });
            }
        }
    }
}
