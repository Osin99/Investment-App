using System.ComponentModel.DataAnnotations;

namespace InvestmentApi.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public TransactionType Type { get; set; } = TransactionType.Buy;

        [Range(0.00000001, double.MaxValue)]
        public decimal Amount { get; set; }

        [Range(0.00001, double.MaxValue)]
        public decimal Price { get; set; }

        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
