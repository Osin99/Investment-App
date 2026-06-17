using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InvestmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetsTransactionsAndPriceSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CoinGeckoId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "CoinGeckoId", "IsActive", "Name", "Symbol" },
                values: new object[,]
                {
                    { 1, "bitcoin", true, "Bitcoin", "BTC" },
                    { 2, "ethereum", true, "Ethereum", "ETH" },
                    { 3, "solana", true, "Solana", "SOL" },
                    { 4, "dogecoin", true, "Dogecoin", "DOGE" },
                    { 5, "shiba-inu", true, "Shiba Inu", "SHIB" },
                    { 6, "ripple", true, "XRP", "XRP" },
                    { 7, "cardano", true, "Cardano", "ADA" },
                    { 8, "chainlink", true, "Chainlink", "LINK" },
                    { 9, "tether", true, "Tether", "USDT" }
                });

            migrationBuilder.CreateTable(
                name: "PriceSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetId = table.Column<int>(type: "INTEGER", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PricePln = table.Column<decimal>(type: "TEXT", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceSnapshots_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Symbol",
                table: "Assets",
                column: "Symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_AssetId_SnapshotDate",
                table: "PriceSnapshots",
                columns: new[] { "AssetId", "SnapshotDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AssetId",
                table: "Transactions",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate",
                table: "Transactions",
                column: "TransactionDate");

            migrationBuilder.Sql("""
                INSERT INTO Assets (Symbol, CoinGeckoId, Name, IsActive)
                SELECT DISTINCT
                    UPPER(TRIM(Symbol)),
                    LOWER(UPPER(TRIM(Symbol))),
                    UPPER(TRIM(Symbol)),
                    1
                FROM Investments
                WHERE UPPER(TRIM(Symbol)) NOT IN (SELECT Symbol FROM Assets);
                """);

            migrationBuilder.Sql("""
                INSERT INTO Transactions (AssetId, Type, Amount, Price, TransactionDate, CreatedAt)
                SELECT
                    a.Id,
                    0,
                    i.Amount,
                    i.BuyPrice,
                    i.BuyDate,
                    datetime('now')
                FROM Investments i
                INNER JOIN Assets a ON a.Symbol = UPPER(TRIM(i.Symbol));
                """);

            migrationBuilder.DropTable(
                name: "Investments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Investments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    BuyDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BuyPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investments", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO Investments (Symbol, Amount, BuyPrice, BuyDate)
                SELECT
                    a.Symbol,
                    t.Amount,
                    t.Price,
                    t.TransactionDate
                FROM Transactions t
                INNER JOIN Assets a ON a.Id = t.AssetId
                WHERE t.Type = 0;
                """);

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "PriceSnapshots");

            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
