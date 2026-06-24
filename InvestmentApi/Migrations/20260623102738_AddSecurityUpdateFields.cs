using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvestmentApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityUpdateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataSource",
                table: "Securities",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "Securities",
                type: "TEXT",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Securities",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Securities",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "DataSource", "Isin", "LastUpdated" },
                values: new object[] { "Seed", null, new DateTime(2026, 6, 23, 9, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "Securities");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "Securities");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Securities");
        }
    }
}
