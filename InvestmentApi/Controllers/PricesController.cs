using Microsoft.AspNetCore.Mvc;
using InvestmentApi.Services;

namespace InvestmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly IPriceService _priceService;

        public PricesController(IPriceService priceService)
        {
            _priceService = priceService;
        }

        [HttpGet("current")]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetCurrentPrices(
            [FromQuery] string? symbols,
            CancellationToken cancellationToken)
        {
            var symbolList = string.IsNullOrWhiteSpace(symbols)
                ? AssetSymbolMapper.GetSeedAssets().Select(a => a.Symbol)
                : symbols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var prices = await _priceService.GetCurrentPricesAsync(symbolList, cancellationToken);

            return Ok(prices.ToDictionary(
                kvp => kvp.Key.ToLowerInvariant(),
                kvp => kvp.Value));
        }
    }
}
