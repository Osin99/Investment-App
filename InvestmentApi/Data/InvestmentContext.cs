using InvestmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentApi.Data
{
    public class InvestmentContext : DbContext
    {
        public InvestmentContext(DbContextOptions<InvestmentContext> options)
            : base(options) { }

        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasIndex(a => a.Symbol).IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.Asset)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(t => t.AssetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.TransactionDate);
            });

            modelBuilder.Entity<PriceSnapshot>(entity =>
            {
                entity.HasOne(p => p.Asset)
                    .WithMany(a => a.PriceSnapshots)
                    .HasForeignKey(p => p.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => new { p.AssetId, p.SnapshotDate }).IsUnique();
            });

            SeedAssets(modelBuilder);
        }

        private static void SeedAssets(ModelBuilder modelBuilder)
        {
            var assets = Services.AssetSymbolMapper.GetSeedAssets()
                .Select((asset, index) => new Asset
                {
                    Id = index + 1,
                    Symbol = asset.Symbol,
                    CoinGeckoId = asset.CoinGeckoId,
                    Name = asset.Name,
                    IsActive = true
                })
                .ToArray();

            modelBuilder.Entity<Asset>().HasData(assets);
        }
    }
}
