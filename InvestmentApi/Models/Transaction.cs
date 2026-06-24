using System.ComponentModel.DataAnnotations;

namespace InvestmentApi.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public TransactionType Type { get; set; } = TransactionType.Buy;

        public AssetCategory Category { get; set; } = AssetCategory.Crypto;

        [Range(0.00000001, double.MaxValue)]
        public decimal Amount { get; set; }

        [Range(0.00001, double.MaxValue)]
        public decimal Price { get; set; }

        // Cena w PLN (dla akcji/ETF w USD/EUR)
        public decimal? PricePln { get; set; }

        // Kurs wymiany użyty do przeliczenia (np. 1 USD = 4.024 PLN)
        public decimal? ExchangeRate { get; set; }

        // Waluta oryginalna instrumentu (PLN, USD, EUR)
        [StringLength(3)]
        public string Currency { get; set; } = "PLN";

        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(10)]
        public string? Unit { get; set; }  // "g", "oz", "szt" itp.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
