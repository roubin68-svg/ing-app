using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierProfileNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessType",
                table: "SupplierProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactMobile",
                table: "SupplierProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactPosition",
                table: "SupplierProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 4, 17, 25, 21, 445, DateTimeKind.Utc).AddTicks(646));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "SupplierProfiles");

            migrationBuilder.DropColumn(
                name: "ContactMobile",
                table: "SupplierProfiles");

            migrationBuilder.DropColumn(
                name: "ContactPosition",
                table: "SupplierProfiles");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 2, 22, 50, 39, 647, DateTimeKind.Utc).AddTicks(7592));
        }
    }
}
