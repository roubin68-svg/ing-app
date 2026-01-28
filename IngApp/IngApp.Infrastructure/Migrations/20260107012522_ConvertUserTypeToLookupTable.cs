using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertUserTypeToLookupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) ایجاد جدول UserTypes
            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            // 2) Seed Data برای UserTypes
            migrationBuilder.InsertData(
                table: "UserTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Buyer", null, true, "خریدار" },
                    { 2, "Supplier", null, true, "تأمین‌کننده" },
                    { 3, "Admin", null, true, "مدیر سیستم" },
                    { 4, "Visitor", null, true, "بازاریاب" }
                });

            // 3) اضافه کردن ستون UserTypeId (موقتاً nullable)
            migrationBuilder.AddColumn<int>(
                name: "UserTypeId",
                table: "Users",
                type: "int",
                nullable: true);

            // 4) Migrate داده‌های موجود: تبدیل Enum به UserTypeId
            // Buyer = 1 → UserTypeId = 1
            // Supplier = 2 → UserTypeId = 2
            // Admin = 3 → UserTypeId = 3
            migrationBuilder.Sql(@"
                UPDATE Users 
                SET UserTypeId = CASE 
                    WHEN UserType = 1 THEN 1  -- Buyer
                    WHEN UserType = 2 THEN 2  -- Supplier
                    WHEN UserType = 3 THEN 3  -- Admin
                    ELSE 1  -- Default to Buyer
                END
            ");

            // 5) حذف ستون UserType (enum)
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");

            // 6) تبدیل UserTypeId به non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "UserTypeId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                columns: new[] { "CreatedAt", "UserTypeId" },
                values: new object[] { new DateTime(2026, 1, 7, 1, 25, 20, 837, DateTimeKind.Utc).AddTicks(6609), 3 });

            // 7) ایجاد Index و Foreign Key
            migrationBuilder.CreateIndex(
                name: "IX_Users_UserTypeId",
                table: "Users",
                column: "UserTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTypes_Code",
                table: "UserTypes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserTypes_UserTypeId",
                table: "Users",
                column: "UserTypeId",
                principalTable: "UserTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserTypes_UserTypeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserTypeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserTypes_Code",
                table: "UserTypes");

            // تبدیل UserTypeId به UserType (enum)
            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE Users 
                SET UserType = UserTypeId
            ");

            migrationBuilder.DropColumn(
                name: "UserTypeId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserTypes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                columns: new[] { "CreatedAt", "UserType" },
                values: new object[] { new DateTime(2026, 1, 4, 22, 11, 50, 902, DateTimeKind.Utc).AddTicks(7749), 3 });
        }
    }
}
