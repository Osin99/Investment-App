namespace InvestmentApi.Models
{
    public class PortfolioHistoryPointDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal MarketValue { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitPercent { get; set; }
        public bool IsPurchaseDate { get; set; }
        public decimal PurchaseInvested { get; set; }
    }

    public class PurchaseHistoryDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BuyPrice { get; set; }
        public string BuyDate { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal MarketValue { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitPercent { get; set; }
    }

    public class PortfolioHistoryDto
    {
        public List<PortfolioHistoryPointDto> History { get; set; } = [];
        public List<PurchaseHistoryDto> Purchases { get; set; } = [];
        public decimal TotalAmount { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal MarketValue { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitPercent { get; set; }
    }
}
