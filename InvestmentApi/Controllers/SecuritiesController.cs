using Microsoft.AspNetCore.Mvc;
using InvestmentApi.Models;
using InvestmentApi.Services;

namespace InvestmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecuritiesController : ControllerBase
    {
        private readonly ISecuritySearchService _securitySearchService;
        private readonly ILogger<SecuritiesController> _logger;

        public SecuritiesController(ISecuritySearchService securitySearchService, ILogger<SecuritiesController> logger)
        {
            _securitySearchService = securitySearchService;
            _logger = logger;
        }

        /// <summary>
        /// Wyszukuje papiery wartościowe (akcje, ETF-y, kryptowaluty) po symbolu lub nazwie
        /// </summary>
        /// <param name="query">Tekst do wyszukania (symbol lub nazwa)</param>
        /// <param name="limit">Maksymalna liczba wyników (domyślnie 20)</param>
        /// <returns>Lista wyników wyszukiwania</returns>
        [HttpGet("search")]
        public async Task<ActionResult<List<SecuritySearchResult>>> Search(
            [FromQuery] string? query,
            [FromQuery] int limit = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest(new { message = "Query parameter is required" });

                if (limit < 1 || limit > 100)
                    return BadRequest(new { message = "Limit must be between 1 and 100" });

                var results = await _securitySearchService.SearchSecuritiesAsync(query, limit, cancellationToken);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching securities");
                return StatusCode(500, new { message = "Error searching securities" });
            }
        }
    }
}
