using InvestmentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestmentApi.Data
{
    public class InvestmentContext : DbContext
    {
        public InvestmentContext(DbContextOptions<InvestmentContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Security> Securities => Set<Security>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Security>(entity =>
            {
                entity.HasIndex(s => s.Symbol).IsUnique();
                entity.HasIndex(s => s.Name);
                entity.HasIndex(s => s.Type); // ← NOWE: Dla filtrowania po typie
            });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasIndex(a => a.Symbol).IsUnique();
                entity.HasIndex(a => a.CoinGeckoId); // ← NOWE: Dla szybkiego wyszukiwania po CoinGeckoId
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasOne(t => t.Asset)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(t => t.AssetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(t => t.TransactionDate);
                entity.HasIndex(t => t.AssetId); // ← NOWE: Dla szybkich zapytań o transakcje
                entity.HasIndex(t => t.UserId); // ← NOWE: Dla filtrowania po użytkowniku
            });

            modelBuilder.Entity<PriceSnapshot>(entity =>
            {
                entity.HasOne(p => p.Asset)
                    .WithMany(a => a.PriceSnapshots)
                    .HasForeignKey(p => p.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => new { p.AssetId, p.SnapshotDate }).IsUnique();
                entity.HasIndex(p => p.FetchedAt); // ← NOWE: Dla cache logic (sprawdzanie świeżości cen)
                entity.HasIndex(p => new { p.AssetId, p.FetchedAt }); // ← NOWE: Dla szybkiego pobierania ostatniej ceny
            });

            SeedAssets(modelBuilder);
            SeedSecurities(modelBuilder);
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

        private static void SeedSecurities(ModelBuilder modelBuilder)
        {
            var createdAt = new DateTime(2026, 6, 23, 9, 0, 0, DateTimeKind.Utc);
            var lastUpdated = new DateTime(2026, 6, 23, 9, 0, 0, DateTimeKind.Utc);
            var securities = new List<Security>
            {
                // Kryptowaluty
                new Security { Id = 1, Symbol = "BTC", Name = "Bitcoin", Type = "Crypto", Currency = "USD", CoinGeckoId = "bitcoin", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 2, Symbol = "ETH", Name = "Ethereum", Type = "Crypto", Currency = "USD", CoinGeckoId = "ethereum", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 3, Symbol = "SOL", Name = "Solana", Type = "Crypto", Currency = "USD", CoinGeckoId = "solana", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 4, Symbol = "DOGE", Name = "Dogecoin", Type = "Crypto", Currency = "USD", CoinGeckoId = "dogecoin", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 5, Symbol = "SHIB", Name = "Shiba Inu", Type = "Crypto", Currency = "USD", CoinGeckoId = "shiba-inu", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 6, Symbol = "XRP", Name = "XRP", Type = "Crypto", Currency = "USD", CoinGeckoId = "ripple", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 7, Symbol = "ADA", Name = "Cardano", Type = "Crypto", Currency = "USD", CoinGeckoId = "cardano", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 8, Symbol = "LINK", Name = "Chainlink", Type = "Crypto", Currency = "USD", CoinGeckoId = "chainlink", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 9, Symbol = "USDT", Name = "Tether", Type = "Crypto", Currency = "USD", CoinGeckoId = "tether", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },

                // Popularne akcje
                new Security { Id = 10, Symbol = "AAPL", Name = "Apple Inc.", Type = "Stock", Exchange = "NASDAQ", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 11, Symbol = "MSFT", Name = "Microsoft Corporation", Type = "Stock", Exchange = "NASDAQ", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 12, Symbol = "GOOGL", Name = "Alphabet Inc.", Type = "Stock", Exchange = "NASDAQ", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 13, Symbol = "AMZN", Name = "Amazon.com Inc.", Type = "Stock", Exchange = "NASDAQ", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 14, Symbol = "TSLA", Name = "Tesla Inc.", Type = "Stock", Exchange = "NASDAQ", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 15, Symbol = "META", Name = "Meta Platforms Inc.", Type = "Stock", Exchange = "NASDAQ", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 16, Symbol = "NVDA", Name = "NVIDIA Corporation", Type = "Stock", Exchange = "NASDAQ", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 17, Symbol = "JPM", Name = "JPMorgan Chase & Co.", Type = "Stock", Exchange = "NYSE", Currency = "USD", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },

                // Popularne ETF-y
                new Security { Id = 18, Symbol = "VWRL", Name = "Vanguard FTSE All-World UCITS ETF", Type = "ETF", Exchange = "XETRA", Currency = "EUR", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 19, Symbol = "CSPX", Name = "iShares Core S&P 500 UCITS ETF", Type = "ETF", Exchange = "XETRA", Currency = "EUR", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 20, Symbol = "VUSA", Name = "Vanguard S&P 500 UCITS ETF", Type = "ETF", Exchange = "XETRA", Currency = "EUR", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 21, Symbol = "EUNL", Name = "iShares MSCI World UCITS ETF", Type = "ETF", Exchange = "XETRA", Currency = "EUR", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 22, Symbol = "VGOV", Name = "Vanguard U.S. Government Bond UCITS ETF", Type = "ETF", Exchange = "XETRA", Currency = "EUR", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
                new Security { Id = 23, Symbol = "IEMG", Name = "iShares MSCI Emerging Markets UCITS ETF", Type = "ETF", Exchange = "XETRA", Currency = "EUR", IsActive = true, CreatedAt = createdAt, LastUpdated = lastUpdated, DataSource = "Seed" },
            };

            modelBuilder.Entity<Security>().HasData(securities);
        }
    }
}
