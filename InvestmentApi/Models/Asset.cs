using System.ComponentModel.DataAnnotations;

namespace InvestmentApi.Models
{
    public class Asset
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string CoinGeckoId { get; set; } = string.Empty;

        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Transaction> Transactions { get; set; } = [];
        public ICollection<PriceSnapshot> PriceSnapshots { get; set; } = [];
    }
}
