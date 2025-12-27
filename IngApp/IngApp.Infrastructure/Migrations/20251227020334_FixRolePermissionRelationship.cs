using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRolePermissionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "PermissionId1",
                table: "RolePermissions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 2, 3, 33, 850, DateTimeKind.Utc).AddTicks(2804));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId1",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("33333333-3333-3333-3333-333333333333") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000002"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000003"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000004"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000005"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000006"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000007"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000008"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-000000000009"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000a"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                column: "PermissionId1",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 27, 1, 41, 44, 79, DateTimeKind.Utc).AddTicks(8817));

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId1",
                table: "RolePermissions",
                column: "PermissionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId1",
                table: "RolePermissions",
                column: "PermissionId1",
                principalTable: "Permissions",
                principalColumn: "Id");
        }
    }
}
