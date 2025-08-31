using System.ComponentModel.DataAnnotations;

namespace InvestmentApi.Models
{
    public class Investment
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Symbol kryptowalut jest wymagany")]
        [StringLength(10, ErrorMessage = "Symbol może mieć maksymalnie 10 znaków")]
        public string Symbol { get; set; } = string.Empty;

        [Range(0.0001, double.MaxValue, ErrorMessage = "Kwota musi być większa niż 0")]
        public decimal Amount { get; set; }

        [Range(0.00001, double.MaxValue, ErrorMessage = "Cena zakupu musi być większa niż 0")]
        public decimal BuyPrice { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Data zakupu jest wymagana")]
        public DateTime BuyDate { get; set; }
    }
}