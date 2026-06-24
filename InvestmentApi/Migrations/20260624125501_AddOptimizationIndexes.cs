using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOptimizationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Securities_Type",
                table: "Securities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_AssetId_FetchedAt",
                table: "PriceSnapshots",
                columns: new[] { "AssetId", "FetchedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceSnapshots_FetchedAt",
                table: "PriceSnapshots",
                column: "FetchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CoinGeckoId",
                table: "Assets",
                column: "CoinGeckoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Securities_Type",
                table: "Securities");

            migrationBuilder.DropIndex(
                name: "IX_PriceSnapshots_AssetId_FetchedAt",
                table: "PriceSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_PriceSnapshots_FetchedAt",
                table: "PriceSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CoinGeckoId",
                table: "Assets");
        }
    }
}
