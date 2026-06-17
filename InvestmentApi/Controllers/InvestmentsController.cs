using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using InvestmentApi.Data;
using InvestmentApi.Models;

namespace InvestmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentsController : ControllerBase
    {
        private readonly InvestmentContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public InvestmentsController(InvestmentContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Investment>>> GetInvestments()
        {
            return await _context.Investments.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Investment>> GetInvestment(int id)
        {
            var investment = await _context.Investments.FindAsync(id);

            if (investment == null)
            {
                return NotFound();
            }

            return investment;
        }

        [HttpPost]
        public async Task<ActionResult<Investment>> PostInvestment(Investment investment)
        {
            _context.Investments.Add(investment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInvestment), new { id = investment.Id }, investment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvestment(int id, Investment updateInvestment)
        {
            if (id != updateInvestment.Id)
            {
                return BadRequest("Nieprawidłowe ID");
            }

            var investment = await _context.Investments.FindAsync(id);
            if (investment == null)
            {
                return NotFound();
            }

            investment.Symbol = updateInvestment.Symbol;
            investment.Amount = updateInvestment.Amount;
            investment.BuyPrice = updateInvestment.BuyPrice;
            investment.BuyDate = updateInvestment.BuyDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInvestment(int id)
        {
            var investment = await _context.Investments.FindAsync(id);
            if (investment == null)
            {
                return NotFound();
            }

            _context.Investments.Remove(investment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<InvestmentSummaryDto>>> GetInvestmentSummary()
        {
            var summary = await _context.Investments
                .GroupBy(i => i.Symbol)
                .Select(g => new InvestmentSummaryDto
                {
                    Symbol = g.Key,
                    TotalAmount = g.Sum(i => i.Amount),
                    TotalInvested = g.Sum(i => i.Amount * i.BuyPrice)
                })
                .ToListAsync();

            return Ok(summary);
        }

        [HttpGet("history")]
        public async Task<ActionResult<PortfolioHistoryDto>> GetPortfolioHistory()
        {
            var investments = await _context.Investments
                .OrderBy(i => i.BuyDate)
                .ThenBy(i => i.Id)
                .ToListAsync();

            if (investments.Count == 0)
            {
                return Ok(new PortfolioHistoryDto());
            }

            var symbols = investments
                .Select(i => i.Symbol.ToUpper())
                .Distinct()
                .ToArray();

            var history = new List<PortfolioHistoryPointDto>();
            var purchases = new List<PurchaseHistoryDto>();

            var firstPurchaseDate = investments.Min(i => i.BuyDate.Date);
            var endDate = DateTime.Today;
            var currentDay = firstPurchaseDate;
            var purchaseIndex = 0;

            decimal totalInvested = 0;
            var amountsBySymbol = new Dictionary<string, decimal>();

            var allHistoricalPrices = await GetAllHistoricalPrices(symbols, firstPurchaseDate, endDate);

            var lastKnownPrices = new Dictionary<string, decimal>();

            while (currentDay <= endDate)
            {
                var purchaseInvested = 0m;

                while (purchaseIndex < investments.Count && investments[purchaseIndex].BuyDate.Date <= currentDay)
                {
                    var investment = investments[purchaseIndex];
                    var invested = investment.Amount * investment.BuyPrice;

                    totalInvested += invested;
                    purchaseInvested += invested;
                    amountsBySymbol[investment.Symbol.ToUpper()] = amountsBySymbol.TryGetValue(investment.Symbol.ToUpper(), out var currentAmount)
                        ? currentAmount + investment.Amount
                        : investment.Amount;

                    purchaseIndex++;
                }

                var dayString = currentDay.ToString("yyyy-MM-dd");
                var marketValue = amountsBySymbol.Sum(amount =>
                {
                    var coinGeckoId = MapToCoinGeckoId(amount.Key);
                    if (!allHistoricalPrices.TryGetValue(coinGeckoId, out var prices))
                        return 0m;

                    decimal price;
                    if (prices.TryGetValue(dayString, out var currentPrice))
                    {
                        price = currentPrice;
                        lastKnownPrices[coinGeckoId] = price;
                    }
                    else if (lastKnownPrices.TryGetValue(coinGeckoId, out var lastPrice))
                    {
                        price = lastPrice;
                    }
                    else
                    {
                        price = 0m;
                    }
                    return amount.Value * price;
                });

                var totalAmount = amountsBySymbol.Values.Sum();
                var profit = marketValue - totalInvested;
                var profitPercent = totalInvested > 0 ? profit / totalInvested * 100 : 0;

                history.Add(new PortfolioHistoryPointDto
                {
                    Date = currentDay.ToString("yyyy-MM-dd"),
                    TotalAmount = totalAmount,
                    TotalInvested = totalInvested,
                    MarketValue = marketValue,
                    Profit = profit,
                    ProfitPercent = Math.Round(profitPercent, 2),
                    IsPurchaseDate = purchaseInvested > 0,
                    PurchaseInvested = purchaseInvested
                });

                currentDay = currentDay.AddDays(1);
            }

            for (var i = 0; i < investments.Count; i++)
            {
                var investment = investments[i];
                var totalAmountUpToPurchase = investments
                    .Where(x => x.BuyDate < investment.BuyDate || (x.BuyDate == investment.BuyDate && x.Id <= investment.Id))
                    .Sum(x => x.Amount);
                var totalInvestedUpToPurchase = investments
                    .Where(x => x.BuyDate < investment.BuyDate || (x.BuyDate == investment.BuyDate && x.Id <= investment.Id))
                    .Sum(x => x.Amount * x.BuyPrice);
                var buyDateString = investment.BuyDate.ToString("yyyy-MM-dd");
                var buyDatePrice = allHistoricalPrices.TryGetValue(MapToCoinGeckoId(investment.Symbol.ToUpper()), out var prices) && prices.TryGetValue(buyDateString, out var historicalPrice)
                    ? historicalPrice
                    : 0m;
                var marketValueUpToPurchase = totalAmountUpToPurchase * buyDatePrice;
                var profitUpToPurchase = marketValueUpToPurchase - totalInvestedUpToPurchase;
                var profitPercentUpToPurchase = totalInvestedUpToPurchase > 0 ? profitUpToPurchase / totalInvestedUpToPurchase * 100 : 0;

                purchases.Add(new PurchaseHistoryDto
                {
                    Id = investment.Id,
                    Symbol = investment.Symbol,
                    Amount = investment.Amount,
                    BuyPrice = investment.BuyPrice,
                    BuyDate = buyDateString,
                    TotalAmount = totalAmountUpToPurchase,
                    TotalInvested = totalInvestedUpToPurchase,
                    MarketValue = marketValueUpToPurchase,
                    Profit = profitUpToPurchase,
                    ProfitPercent = Math.Round(profitPercentUpToPurchase, 2)
                });
            }

            var lastPoint = history[^1];

            return Ok(new PortfolioHistoryDto
            {
                History = history,
                Purchases = purchases,
                TotalAmount = lastPoint.TotalAmount,
                TotalInvested = lastPoint.TotalInvested,
                MarketValue = lastPoint.MarketValue,
                Profit = lastPoint.Profit,
                ProfitPercent = lastPoint.ProfitPercent
            });
        }

        private async Task<Dictionary<string, Dictionary<string, decimal>>> GetAllHistoricalPrices(IEnumerable<string> symbols, DateTime firstPurchaseDate, DateTime endDate)
        {
            var ids = symbols
                .Select(MapToCoinGeckoId)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
            {
                return new Dictionary<string, Dictionary<string, decimal>>();
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("InvestmentApp/1.0");

            var result = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var id in ids)
            {
                var pricesByDate = new Dictionary<string, decimal>();
                var chunkStart = firstPurchaseDate.Date;

                while (chunkStart <= endDate.Date)
                {
                    var chunkEnd = chunkStart.AddDays(89);
                    if (chunkEnd > endDate.Date)
                        chunkEnd = endDate.Date;

                    try
                    {
                        var from = new DateTimeOffset(chunkStart).ToUnixTimeSeconds();
                        var to = chunkEnd.AddDays(1).AddTicks(-1);
                        var toUnix = new DateTimeOffset(to).ToUnixTimeSeconds();
                        var url = $"https://api.coingecko.com/api/v3/coins/{id}/market_chart/range?vs_currency=pln&from={from}&to={toUnix}";
                        var response = await client.GetFromJsonAsync<MarketChartResponse>(url);
                        if (response?.Prices != null)
                        {
                            foreach (var point in response.Prices.Where(p => p.Count >= 2))
                            {
                                var date = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeMilliseconds((long)point[0]).UtcDateTime, TimeZoneInfo.Local).Date.ToString("yyyy-MM-dd");
                                var price = (decimal)point[1];

                                if (!pricesByDate.ContainsKey(date))
                                    pricesByDate[date] = price;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Nie udało się pobrać historii dla {id} z zakresu {chunkStart:yyyy-MM-dd} do {chunkEnd:yyyy-MM-dd}: {ex.Message}");
                    }

                    chunkStart = chunkEnd.AddDays(1);
                    await Task.Delay(1200);
                }

                result[id] = pricesByDate;
            }

            return result;
        }

        private class MarketChartResponse
        {
            public List<List<double>> Prices { get; set; } = new();
        }

        private static string MapToCoinGeckoId(string symbol)
        {
            var map = new Dictionary<string, string>
            {
                ["BTC"] = "bitcoin",
                ["ETH"] = "ethereum",
                ["SOL"] = "solana",
                ["DOGE"] = "dogecoin",
                ["SHIB"] = "shiba-inu",
                ["XRP"] = "ripple",
                ["ADA"] = "cardano",
                ["LINK"] = "chainlink",
                ["USDT"] = "tether"
            };

            return map.TryGetValue(symbol.ToUpper(), out var id) ? id : symbol.ToLower();
        }
    }
}
