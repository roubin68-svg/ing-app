using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferManagePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "DisplayName", "IsActive" },
                values: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), "Offer.Manage", "", "مدیریت آگهی‌ها", true });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 1, 41, 44, 79, DateTimeKind.Utc).AddTicks(8817));

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-00000000000b"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 0, 2, 30, 790, DateTimeKind.Utc).AddTicks(6770));
        }
    }
}
