namespace InvestmentApi.Models
{
    public class PriceSnapshot
    {
        public long Id { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public DateOnly SnapshotDate { get; set; }

        public decimal PricePln { get; set; }

        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }
}
