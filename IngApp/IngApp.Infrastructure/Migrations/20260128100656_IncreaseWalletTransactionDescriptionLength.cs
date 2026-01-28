using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseWalletTransactionDescriptionLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WalletTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 10, 6, 55, 400, DateTimeKind.Utc).AddTicks(2049));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 10, 6, 55, 400, DateTimeKind.Utc).AddTicks(2052));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(1997));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(2001));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(2004));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(2006));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(7212), new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(7206) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(7216), new DateTime(2026, 1, 28, 10, 6, 55, 405, DateTimeKind.Utc).AddTicks(7215) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 10, 6, 55, 425, DateTimeKind.Utc).AddTicks(2338));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WalletTransactions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 616, DateTimeKind.Utc).AddTicks(2483));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 616, DateTimeKind.Utc).AddTicks(2486));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3983));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3986));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3988));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3990));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6319), new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6315) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6321), new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6321) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 631, DateTimeKind.Utc).AddTicks(6053));
        }
    }
}
