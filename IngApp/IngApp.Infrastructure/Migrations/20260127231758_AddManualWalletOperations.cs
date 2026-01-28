using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualWalletOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 899, DateTimeKind.Utc).AddTicks(2823));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 899, DateTimeKind.Utc).AddTicks(2825));

            migrationBuilder.InsertData(
                table: "FinancialOperationTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 6, "ManualDeposit", "واریز دستی توسط مدیر", true, "واریز دستی" },
                    { 7, "ManualWithdrawal", "برداشت دستی توسط مدیر", true, "برداشت دستی" }
                });

            migrationBuilder.InsertData(
                table: "FinancialReferenceTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[] { 6, "AdminAction", "مرجع: عملیات دستی توسط مدیر", true, "عملیات مدیر" });

            // فقط Financial.Manage را اضافه می‌کنیم (Visitor.View و Visitor.Manage قبلاً اضافه شده‌اند)
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "DisplayName", "IsActive" },
                values: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000e"), "Financial.Manage", "", "مدیریت مالی", true });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(500));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(502));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(504));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(506));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2824), new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2819) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2826), new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2826) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 915, DateTimeKind.Utc).AddTicks(1750));

            // فقط RolePermission برای Financial.Manage را اضافه می‌کنیم
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000e"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FinancialOperationTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "FinancialOperationTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "FinancialReferenceTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000e"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-00000000000e"));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 1, 27, 390, DateTimeKind.Utc).AddTicks(5614));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 1, 27, 390, DateTimeKind.Utc).AddTicks(5617));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(1691));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(1696));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(1700));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(1704));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(8301), new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(8292) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(8305), new DateTime(2026, 1, 27, 18, 1, 27, 399, DateTimeKind.Utc).AddTicks(8304) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 1, 27, 423, DateTimeKind.Utc).AddTicks(9429));
        }
    }
}
