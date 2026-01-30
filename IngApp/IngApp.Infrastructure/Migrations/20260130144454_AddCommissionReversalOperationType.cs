using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionReversalOperationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 518, DateTimeKind.Local).AddTicks(1256));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 518, DateTimeKind.Local).AddTicks(1259));

            migrationBuilder.InsertData(
                table: "FinancialOperationTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[] { 11, "CommissionReversal", "برگشت پورسانت پرداخت شده", true, "برگشت پورسانت" });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(75));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(78));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(81));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(83));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(2368), new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(2364) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(2371), new DateTime(2026, 1, 30, 18, 14, 53, 521, DateTimeKind.Local).AddTicks(2370) });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 532, DateTimeKind.Local).AddTicks(6393));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 18, 14, 53, 533, DateTimeKind.Local).AddTicks(1171));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FinancialOperationTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 446, DateTimeKind.Local).AddTicks(1128));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 446, DateTimeKind.Local).AddTicks(1132));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 450, DateTimeKind.Local).AddTicks(9990));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 450, DateTimeKind.Local).AddTicks(9993));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 450, DateTimeKind.Local).AddTicks(9995));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 450, DateTimeKind.Local).AddTicks(9997));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 30, 13, 4, 34, 451, DateTimeKind.Local).AddTicks(2714), new DateTime(2026, 1, 30, 13, 4, 34, 451, DateTimeKind.Local).AddTicks(2705) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 30, 13, 4, 34, 451, DateTimeKind.Local).AddTicks(2717), new DateTime(2026, 1, 30, 13, 4, 34, 451, DateTimeKind.Local).AddTicks(2716) });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 467, DateTimeKind.Local).AddTicks(136));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 467, DateTimeKind.Local).AddTicks(4864));
        }
    }
}
