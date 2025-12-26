using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierCategoryAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupplierCategoryAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCategoryAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierCategoryAccesses_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 19, 18, 35, 59, 297, DateTimeKind.Utc).AddTicks(1414));

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategoryAccesses_ProductCategoryId",
                table: "SupplierCategoryAccesses",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategoryAccesses_UserId_ProductCategoryId",
                table: "SupplierCategoryAccesses",
                columns: new[] { "UserId", "ProductCategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierCategoryAccesses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 16, 22, 15, 48, 214, DateTimeKind.Utc).AddTicks(1273));
        }
    }
}
