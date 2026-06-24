using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Transactions",
                type: "TEXT",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Transactions");
        }
    }
}
