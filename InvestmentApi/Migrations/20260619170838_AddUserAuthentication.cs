using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Najpierw tworzymy tabelę Users
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            // 2. Wstawiamy domyślnego użytkownika admin (Id=1)
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "PasswordHash", "Role", "CreatedAt", "IsActive" },
                values: new object[] {
                    1,
                    "admin@investment.app",
                    // SHA256 hash of "Admin123!" 
                    "PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=",
                    "Admin",
                    DateTime.UtcNow,
                    true
                });

            // 3. Dodajemy kolumnę UserId do Transactions (nullable tymczasowo)
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Transactions",
                type: "INTEGER",
                nullable: true,
                defaultValue: null);

            // 4. Przypisujemy wszystkie istniejące transakcje do admina (Id=1)
            migrationBuilder.Sql("UPDATE Transactions SET UserId = 1 WHERE UserId IS NULL");

            // 5. Dodajemy kolumnę UserId do Assets (nullable)
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Assets",
                type: "INTEGER",
                nullable: true);

            // 6. Tworzymy indeksy
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_UserId",
                table: "Assets",
                column: "UserId");

            // 7. Dodajemy klucze obce
            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Users_UserId",
                table: "Assets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Users_UserId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Assets_UserId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Assets");
        }
    }
}
