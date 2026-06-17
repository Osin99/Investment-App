using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvestmentApi.Data;
using InvestmentApi.Models;
using InvestmentApi.Services;

namespace InvestmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentsController : ControllerBase
    {
        private readonly InvestmentContext _context;
        private readonly IPortfolioHistoryService _portfolioHistoryService;

        public InvestmentsController(
            InvestmentContext context,
            IPortfolioHistoryService portfolioHistoryService)
        {
            _context = context;
            _portfolioHistoryService = portfolioHistoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvestmentDto>>> GetInvestments()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Asset)
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            return transactions.Select(InvestmentDto.FromTransaction).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InvestmentDto>> GetInvestment(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound();

            return InvestmentDto.FromTransaction(transaction);
        }

        [HttpPost]
        public async Task<ActionResult<InvestmentDto>> PostInvestment(InvestmentDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            dto.Symbol = AssetSymbolMapper.NormalizeSymbol(dto.Symbol);
            var asset = await GetOrCreateAssetAsync(dto.Symbol);
            var transaction = MapToTransaction(dto, asset.Id);

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _context.Entry(transaction).Reference(t => t.Asset).LoadAsync();

            return CreatedAtAction(nameof(GetInvestment), new { id = transaction.Id }, InvestmentDto.FromTransaction(transaction));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvestment(int id, InvestmentDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Nieprawidłowe ID");

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var transaction = await _context.Transactions
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound();

            dto.Symbol = AssetSymbolMapper.NormalizeSymbol(dto.Symbol);
            var asset = await GetOrCreateAssetAsync(dto.Symbol);

            transaction.AssetId = asset.Id;
            transaction.Type = dto.Type;
            transaction.Amount = dto.Amount;
            transaction.Price = dto.BuyPrice;
            transaction.TransactionDate = dto.BuyDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvestment(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<InvestmentSummaryDto>>> GetInvestmentSummary()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Asset)
                .ToListAsync();

            var holdings = new Dictionary<string, (decimal Amount, decimal Invested)>(StringComparer.OrdinalIgnoreCase);

            foreach (var transaction in transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.Id))
            {
                var symbol = transaction.Asset.Symbol;

                if (!holdings.TryGetValue(symbol, out var holding))
                    holding = (0, 0);

                if (transaction.Type == TransactionType.Buy)
                {
                    holding.Amount += transaction.Amount;
                    holding.Invested += transaction.Amount * transaction.Price;
                }
                else if (holding.Amount > 0)
                {
                    var sellAmount = Math.Min(transaction.Amount, holding.Amount);
                    var costBasis = sellAmount * (holding.Invested / holding.Amount);
                    holding.Amount -= sellAmount;
                    holding.Invested -= costBasis;
                }

                holdings[symbol] = holding;
            }

            var summary = holdings
                .Where(h => h.Value.Amount > 0)
                .Select(h => new InvestmentSummaryDto
                {
                    Symbol = h.Key,
                    TotalAmount = h.Value.Amount,
                    TotalInvested = h.Value.Invested
                })
                .OrderBy(s => s.Symbol)
                .ToList();

            return Ok(summary);
        }

        [HttpGet("history")]
        public async Task<ActionResult<PortfolioHistoryDto>> GetPortfolioHistory(CancellationToken cancellationToken)
        {
            var history = await _portfolioHistoryService.BuildHistoryAsync(cancellationToken);
            return Ok(history);
        }

        private async Task<Asset> GetOrCreateAssetAsync(string symbol)
        {
            var normalized = AssetSymbolMapper.NormalizeSymbol(symbol);
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Symbol == normalized);

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
            await _context.SaveChangesAsync();
            return asset;
        }

        private static Transaction MapToTransaction(InvestmentDto dto, int assetId) => new()
        {
            AssetId = assetId,
            Type = dto.Type,
            Amount = dto.Amount,
            Price = dto.BuyPrice,
            TransactionDate = dto.BuyDate,
            CreatedAt = DateTime.UtcNow
        };
    }
}
