using System.ComponentModel.DataAnnotations;

namespace InvestmentApi.Models
{
    public class Security
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "Crypto";  // "Stock", "ETF", "Crypto", "Other"

        [StringLength(50)]
        public string? Exchange { get; set; }  // NASDAQ, XETRA, null dla crypto

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(50)]
        public string? CoinGeckoId { get; set; }  // Dla kryptowalut

        [StringLength(12)]
        public string? Isin { get; set; }  // Kod ISIN dla akcji/ETF-ów (może być null - Finnhub wymaga płatnego planu)

        [StringLength(50)]
        public string? Figi { get; set; }  // Bloomberg FIGI - unikalny identyfikator instrumentu (używamy zamiast ISIN)

        [StringLength(10)]
        public string? DefaultUnit { get; set; }  // "g", "oz", "szt" itp.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? DataSource { get; set; }  // "Finnhub", "CoinGecko", itp.

        // Relacje
        public ICollection<Asset> Assets { get; set; } = [];
    }
}
