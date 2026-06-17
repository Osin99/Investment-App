using System.ComponentModel.DataAnnotations;

namespace InvestmentApi.Models
{
    /// <summary>
    /// API contract kept for frontend compatibility — maps to Transaction + Asset.
    /// </summary>
    public class InvestmentDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Symbol kryptowaluty jest wymagany")]
        [StringLength(10, ErrorMessage = "Symbol może mieć maksymalnie 10 znaków")]
        public string Symbol { get; set; } = string.Empty;

        public TransactionType Type { get; set; } = TransactionType.Buy;

        [Range(0.0001, double.MaxValue, ErrorMessage = "Kwota musi być większa niż 0")]
        public decimal Amount { get; set; }

        [Range(0.00001, double.MaxValue, ErrorMessage = "Cena musi być większa niż 0")]
        public decimal BuyPrice { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Data transakcji jest wymagana")]
        public DateTime BuyDate { get; set; }

        public static InvestmentDto FromTransaction(Transaction transaction) => new()
        {
            Id = transaction.Id,
            Symbol = transaction.Asset.Symbol,
            Type = transaction.Type,
            Amount = transaction.Amount,
            BuyPrice = transaction.Price,
            BuyDate = transaction.TransactionDate
        };
    }
}
