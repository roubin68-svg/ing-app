using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionCancellationRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2002);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2003);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2004);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2005);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2006);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 2000);

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "String"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

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

            migrationBuilder.InsertData(
                table: "FinancialOperationTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[] { 10, "SubscriptionRefund", "برگشت مبلغ اشتراک لغو شده", true, "برگشت مبلغ اشتراک" });

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

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "CreatedAt", "DataType", "Description", "DisplayName", "Key", "UpdatedAt", "Value" },
                values: new object[] { 1, new DateTime(2026, 1, 30, 13, 4, 34, 467, DateTimeKind.Local).AddTicks(136), "Number", "درصد کارمزد خدمات که از مبلغ برگشتی اشتراک کسر می‌شود", "کارمزد خدمات لغو اشتراک (درصد)", "SubscriptionCancellationServiceFeePercentage", null, "10" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 30, 13, 4, 34, 467, DateTimeKind.Local).AddTicks(4864));

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DeleteData(
                table: "FinancialOperationTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 20, 52, 24, 243, DateTimeKind.Local).AddTicks(7722));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 20, 52, 24, 243, DateTimeKind.Local).AddTicks(7734));

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[] { 2000, "BarChartOutlined", true, "reports", 9, null, null, "#", "گزارش‌ها" });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 20, 52, 24, 249, DateTimeKind.Local).AddTicks(8257));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 20, 52, 24, 249, DateTimeKind.Local).AddTicks(8260));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 20, 52, 24, 249, DateTimeKind.Local).AddTicks(8262));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 20, 52, 24, 249, DateTimeKind.Local).AddTicks(8264));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 29, 20, 52, 24, 257, DateTimeKind.Local).AddTicks(5691), new DateTime(2026, 1, 29, 20, 52, 24, 257, DateTimeKind.Local).AddTicks(5687) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 29, 20, 52, 24, 257, DateTimeKind.Local).AddTicks(5695), new DateTime(2026, 1, 29, 20, 52, 24, 257, DateTimeKind.Local).AddTicks(5694) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 20, 52, 24, 286, DateTimeKind.Local).AddTicks(2473));

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[,]
                {
                    { 2001, null, true, "reports-financial-transactions", 1, 2000, null, "/wallet-transactions-report", "گزارش تراکنش‌های مالی" },
                    { 2002, null, true, "reports-commissions", 2, 2000, null, "/commissions-report", "گزارش پورسانت‌ها" },
                    { 2003, null, true, "reports-income-expense", 3, 2000, null, "/reports/income-expense", "گزارش درآمد/هزینه" },
                    { 2004, null, true, "reports-bank-transactions", 4, 2000, null, "/reports/bank-transactions", "گزارش تراکنش‌های بانکی" },
                    { 2005, null, true, "reports-subscriptions", 5, 2000, null, "/reports/subscriptions", "گزارش اشتراک‌ها" },
                    { 2006, null, true, "reports-users", 6, 2000, null, "/reports/users", "گزارش کاربران" }
                });
        }
    }
}
