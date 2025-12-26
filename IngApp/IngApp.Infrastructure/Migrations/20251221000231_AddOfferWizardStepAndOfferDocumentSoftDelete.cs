using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferWizardStepAndOfferDocumentSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WizardStep",
                table: "Offers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OfferDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OfferDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 21, 0, 2, 30, 790, DateTimeKind.Utc).AddTicks(6770));

            migrationBuilder.CreateIndex(
                name: "IX_OfferDocuments_OfferId_IsDeleted",
                table: "OfferDocuments",
                columns: new[] { "OfferId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OfferDocuments_OfferId_IsDeleted",
                table: "OfferDocuments");

            migrationBuilder.DropColumn(
                name: "WizardStep",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OfferDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OfferDocuments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2025, 12, 20, 1, 20, 18, 865, DateTimeKind.Utc).AddTicks(7843));
        }
    }
}
