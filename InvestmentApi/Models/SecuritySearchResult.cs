namespace InvestmentApi.Models
{
    public class SecuritySearchResult
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;  // "Stock", "ETF", "Crypto", "Other"
        public string? Exchange { get; set; }
        public string Currency { get; set; } = "USD";
        public string? CoinGeckoId { get; set; }
    }
}
