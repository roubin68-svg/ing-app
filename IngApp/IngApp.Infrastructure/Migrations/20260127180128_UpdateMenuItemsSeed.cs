using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMenuItemsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ابتدا ParentId منوی 10 را null می‌کنیم تا بتوانیم آن را حذف کنیم
            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 10,
                column: "ParentId",
                value: null);

            // حالا می‌توانیم منوها را حذف کنیم
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 10);

            // ابتدا ParentId منوهای وابسته به 11 را null می‌کنیم
            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 12,
                column: "ParentId",
                value: null);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 5);

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
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Key", "Title" },
                values: new object[] { "products-categories", "دسته‌بندی محصولات" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 6,
                column: "Order",
                value: 5);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Order", "ParentId", "RequiredPermissionCode", "Title" },
                values: new object[] { 3, null, null, "نوع تامین کننده" });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[,]
                {
                    { 15, null, true, "kyc-templates", 4, null, null, "/kyc-templates", "قالب‌های KYC" },
                    { 18, "FileTextOutlined", true, "offer-managment", 2, null, "Offer.Manage", "/offer-managment", "مدیریت آگهی ها" },
                    { 22, "WalletOutlined", true, "financial", 6, null, null, "#", "سیستم مالی" },
                    { 1007, null, true, "visitor-management", 7, null, null, "/visitor-management", "مدیریت بازاریابان" },
                    { 1008, null, true, "buyer-profiles", 8, null, null, "/buyer-profiles", "پروفایل خریداران" }
                });

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

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[,]
                {
                    { 1001, null, true, "subscriptions", 1, 22, null, "/subscriptions", "اشتراک‌ها" },
                    { 1002, null, true, "top-up", 2, 22, null, "/top-up", "شارژ کیف پول" },
                    { 1003, null, true, "wallet-transactions", 3, 22, null, "/wallet-transactions", "تراکنش‌های کیف پول" },
                    { 1006, null, true, "commission-rules", 4, 22, null, "/commission-rules", "قوانین پورسانت" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1003);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1006);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1007);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 1008);

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 17, 15, 44, 973, DateTimeKind.Utc).AddTicks(2658));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 17, 15, 44, 973, DateTimeKind.Utc).AddTicks(2660));

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Key", "Title" },
                values: new object[] { "category-management", "مدیریت دسته‌بندی‌ها" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 6,
                column: "Order",
                value: 3);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Order", "ParentId", "RequiredPermissionCode", "Title" },
                values: new object[] { 2, 11, "SupplierType.Manage", "مدیریت نوع تأمین‌کننده" });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[,]
                {
                    { 5, "SettingOutlined", true, "settings", 4, null, "Settings.View", "#", "تنظیمات" },
                    { 11, "TeamOutlined", true, "suppliers", 5, null, "Supplier.View", "#", "مدیریت تأمین‌کنندگان" }
                });

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8608));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8610));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8613));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8615));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(940), new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(936) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(942), new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(942) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 17, 15, 44, 988, DateTimeKind.Utc).AddTicks(1020));

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[] { 10, null, true, "menu-settings", 2, 5, "Menu.Manage", "/menu-settings", "تنظیمات منو" });
        }
    }
}
