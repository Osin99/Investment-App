using InvestmentApi.Data;
using InvestmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentApi.Services
{
    public class PortfolioHistoryService : IPortfolioHistoryService
    {
        private readonly InvestmentContext _context;
        private readonly IPriceService _priceService;

        public PortfolioHistoryService(InvestmentContext context, IPriceService priceService)
        {
            _context = context;
            _priceService = priceService;
        }

        public async Task<PortfolioHistoryDto> BuildHistoryAsync(CancellationToken cancellationToken = default)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Asset)
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.Id)
                .ToListAsync(cancellationToken);

            if (transactions.Count == 0)
                return new PortfolioHistoryDto();

            var symbols = transactions
                .Select(t => t.Asset.Symbol)
                .Distinct()
                .ToArray();

            var firstDate = transactions.Min(t => t.TransactionDate.Date);
            var endDate = DateTime.Today;

            var allHistoricalPrices = await _priceService.GetHistoricalPricesAsync(
                symbols, firstDate, endDate, cancellationToken);

            var history = new List<PortfolioHistoryPointDto>();
            var purchases = new List<PurchaseHistoryDto>();

            var currentDay = firstDate;
            var transactionIndex = 0;

            decimal totalInvested = 0;
            var amountsBySymbol = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var investedBySymbol = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var lastKnownPrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            while (currentDay <= endDate)
            {
                var dayInvested = 0m;

                while (transactionIndex < transactions.Count &&
                       transactions[transactionIndex].TransactionDate.Date <= currentDay)
                {
                    dayInvested += ApplyTransaction(
                        transactions[transactionIndex],
                        amountsBySymbol,
                        investedBySymbol,
                        ref totalInvested);
                    transactionIndex++;
                }

                var dayString = currentDay.ToString("yyyy-MM-dd");
                var marketValue = CalculateMarketValue(amountsBySymbol, allHistoricalPrices, dayString, lastKnownPrices);
                var totalAmount = amountsBySymbol.Values.Sum();
                var profit = marketValue - totalInvested;
                var profitPercent = totalInvested > 0 ? profit / totalInvested * 100 : 0;

                history.Add(new PortfolioHistoryPointDto
                {
                    Date = dayString,
                    TotalAmount = totalAmount,
                    TotalInvested = totalInvested,
                    MarketValue = marketValue,
                    Profit = profit,
                    ProfitPercent = Math.Round(profitPercent, 2),
                    IsPurchaseDate = dayInvested > 0,
                    PurchaseInvested = dayInvested
                });

                currentDay = currentDay.AddDays(1);
            }

            foreach (var transaction in transactions)
            {
                purchases.Add(BuildPurchaseSnapshot(transaction, transactions, allHistoricalPrices));
            }

            var lastPoint = history[^1];

            return new PortfolioHistoryDto
            {
                History = history,
                Purchases = purchases,
                TotalAmount = lastPoint.TotalAmount,
                TotalInvested = lastPoint.TotalInvested,
                MarketValue = lastPoint.MarketValue,
                Profit = lastPoint.Profit,
                ProfitPercent = lastPoint.ProfitPercent
            };
        }

        private static decimal ApplyTransaction(
            Transaction transaction,
            Dictionary<string, decimal> amountsBySymbol,
            Dictionary<string, decimal> investedBySymbol,
            ref decimal totalInvested)
        {
            var symbol = transaction.Asset.Symbol;

            if (transaction.Type == TransactionType.Buy)
            {
                var invested = transaction.Amount * transaction.Price;
                totalInvested += invested;

                amountsBySymbol[symbol] = amountsBySymbol.GetValueOrDefault(symbol) + transaction.Amount;
                investedBySymbol[symbol] = investedBySymbol.GetValueOrDefault(symbol) + invested;
                return invested;
            }

            var currentAmount = amountsBySymbol.GetValueOrDefault(symbol);
            var currentInvested = investedBySymbol.GetValueOrDefault(symbol);
            var sellAmount = Math.Min(transaction.Amount, currentAmount);

            if (sellAmount > 0 && currentAmount > 0)
            {
                var costBasis = sellAmount * (currentInvested / currentAmount);
                totalInvested -= costBasis;
                amountsBySymbol[symbol] = currentAmount - sellAmount;
                investedBySymbol[symbol] = currentInvested - costBasis;

                if (amountsBySymbol[symbol] <= 0)
                {
                    amountsBySymbol.Remove(symbol);
                    investedBySymbol.Remove(symbol);
                }
            }

            return 0m;
        }

        private static decimal CalculateMarketValue(
            Dictionary<string, decimal> amountsBySymbol,
            Dictionary<string, Dictionary<string, decimal>> allHistoricalPrices,
            string dayString,
            Dictionary<string, decimal> lastKnownPrices)
        {
            return amountsBySymbol.Sum(entry =>
            {
                var coinGeckoId = AssetSymbolMapper.ToCoinGeckoId(entry.Key);

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

                return entry.Value * price;
            });
        }

        private static PurchaseHistoryDto BuildPurchaseSnapshot(
            Transaction transaction,
            List<Transaction> allTransactions,
            Dictionary<string, Dictionary<string, decimal>> allHistoricalPrices)
        {
            var amountsBySymbol = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var investedBySymbol = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            decimal totalInvested = 0;

            foreach (var tx in allTransactions)
            {
                if (tx.TransactionDate > transaction.TransactionDate ||
                    (tx.TransactionDate == transaction.TransactionDate && tx.Id > transaction.Id))
                    break;

                ApplyTransaction(tx, amountsBySymbol, investedBySymbol, ref totalInvested);
            }

            var dayString = transaction.TransactionDate.ToString("yyyy-MM-dd");
            var lastKnownPrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var marketValue = CalculateMarketValue(amountsBySymbol, allHistoricalPrices, dayString, lastKnownPrices);
            var totalAmount = amountsBySymbol.Values.Sum();
            var profit = marketValue - totalInvested;
            var profitPercent = totalInvested > 0 ? profit / totalInvested * 100 : 0;

            return new PurchaseHistoryDto
            {
                Id = transaction.Id,
                Symbol = transaction.Asset.Symbol,
                Type = transaction.Type,
                Amount = transaction.Amount,
                BuyPrice = transaction.Price,
                BuyDate = dayString,
                TotalAmount = totalAmount,
                TotalInvested = totalInvested,
                MarketValue = marketValue,
                Profit = profit,
                ProfitPercent = Math.Round(profitPercent, 2)
            };
        }
    }
}
