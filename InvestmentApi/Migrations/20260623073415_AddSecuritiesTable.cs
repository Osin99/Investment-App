using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InvestmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSecuritiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecurityId",
                table: "Assets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Securities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Exchange = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CoinGeckoId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DefaultUnit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Securities", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 1,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 2,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 3,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 4,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 5,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 6,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 7,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 8,
                column: "SecurityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 9,
                column: "SecurityId",
                value: null);

            migrationBuilder.InsertData(
                table: "Securities",
                columns: new[] { "Id", "CoinGeckoId", "CreatedAt", "Currency", "DefaultUnit", "Exchange", "IsActive", "Name", "Symbol", "Type" },
                values: new object[,]
                {
                    { 1, "bitcoin", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Bitcoin", "BTC", 2 },
                    { 2, "ethereum", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Ethereum", "ETH", 2 },
                    { 3, "solana", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Solana", "SOL", 2 },
                    { 4, "dogecoin", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Dogecoin", "DOGE", 2 },
                    { 5, "shiba-inu", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Shiba Inu", "SHIB", 2 },
                    { 6, "ripple", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "XRP", "XRP", 2 },
                    { 7, "cardano", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Cardano", "ADA", 2 },
                    { 8, "chainlink", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Chainlink", "LINK", 2 },
                    { 9, "tether", new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, null, true, "Tether", "USDT", 2 },
                    { 10, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NASDAQ", true, "Apple Inc.", "AAPL", 0 },
                    { 11, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NASDAQ", true, "Microsoft Corporation", "MSFT", 0 },
                    { 12, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NASDAQ", true, "Alphabet Inc.", "GOOGL", 0 },
                    { 13, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NASDAQ", true, "Amazon.com Inc.", "AMZN", 0 },
                    { 14, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NASDAQ", true, "Tesla Inc.", "TSLA", 0 },
                    { 15, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NASDAQ", true, "Meta Platforms Inc.", "META", 0 },
                    { 16, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NASDAQ", true, "NVIDIA Corporation", "NVDA", 0 },
                    { 17, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "USD", null, "NYSE", true, "JPMorgan Chase & Co.", "JPM", 0 },
                    { 18, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "EUR", null, "XETRA", true, "Vanguard FTSE All-World UCITS ETF", "VWRL", 1 },
                    { 19, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "EUR", null, "XETRA", true, "iShares Core S&P 500 UCITS ETF", "CSPX", 1 },
                    { 20, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "EUR", null, "XETRA", true, "Vanguard S&P 500 UCITS ETF", "VUSA", 1 },
                    { 21, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "EUR", null, "XETRA", true, "iShares MSCI World UCITS ETF", "EUNL", 1 },
                    { 22, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "EUR", null, "XETRA", true, "Vanguard U.S. Government Bond UCITS ETF", "VGOV", 1 },
                    { 23, null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc), "EUR", null, "XETRA", true, "iShares MSCI Emerging Markets UCITS ETF", "IEMG", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SecurityId",
                table: "Assets",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_Securities_Name",
                table: "Securities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Securities_Symbol",
                table: "Securities",
                column: "Symbol",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Securities_SecurityId",
                table: "Assets",
                column: "SecurityId",
                principalTable: "Securities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Securities_SecurityId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "Securities");

            migrationBuilder.DropIndex(
                name: "IX_Assets_SecurityId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "SecurityId",
                table: "Assets");
        }
    }
}
