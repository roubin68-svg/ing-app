using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "DisplayName", "IsActive" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), "Settings.View", "", "مشاهده تنظیمات", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000002"), "User.Manage", "", "مدیریت کاربران", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000003"), "Role.Manage", "", "مدیریت نقش‌ها", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000004"), "Permission.Manage", "", "مدیریت دسترسی‌ها", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000005"), "Menu.Manage", "", "مدیریت منوها", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000006"), "Product.ViewAll", "", "مشاهده محصولات", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000007"), "ProductCategory.Manage", "", "مدیریت دسته‌بندی محصولات", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000008"), "SupplierType.Manage", "", "مدیریت نوع تأمین‌کننده", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000009"), "Supplier.Manage", "", "مدیریت تأمین‌کنندگان", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000a"), "Kyc.Review", "", "بررسی مدارک KYC", true }
                });

            migrationBuilder.InsertData(
                schema: "IngAppUser",
                table: "Roles",
                columns: new[] { "Id", "Description", "DisplayName", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), "دسترسی‌های پایه کاربر", "خریدار", true, "Buyer" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "دسترسی‌های پنل تأمین‌کننده", "تأمین‌کننده", true, "Supplier" },
                  //  { new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), "دسترسی کامل به سیستم", "ادمین", true, "Admin" }
                });

            //migrationBuilder.InsertData(
            //    table: "Users",
            //    columns: new[] { "Id", "CreatedAt", "DisplayName", "IsActive", "PhoneNumber", "SubscriptionLevel", "UpdatedAt", "UserType", "VerificationStatus" },
            //    values: new object[] { new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"), new DateTime(2025, 12, 16, 8, 52, 58, 476, DateTimeKind.Utc).AddTicks(3016), "علی هور", true, "09123823632", 0, null, 3, 0 });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "PermissionId1" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000002"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000003"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000004"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000005"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000006"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000007"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000008"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000009"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null },
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000a"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), null }
                });

            //migrationBuilder.InsertData(
            //    table: "UserRoles",
            //    columns: new[] { "RoleId", "UserId" },
            //    values: new object[] { new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000002"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000003"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000004"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000005"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000006"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000007"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000008"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000009"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000a"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                schema: "IngAppUser",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                schema: "IngAppUser",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-00000000000a"));

            migrationBuilder.DeleteData(
                schema: "IngAppUser",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"));
        }
    }
}
