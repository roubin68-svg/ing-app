using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDocumentIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KycTemplates_SupplierTypes_SupplierTypeId",
                table: "KycTemplates");

            migrationBuilder.DropIndex(
                name: "IX_UserDocuments_UserId",
                table: "UserDocuments");

            migrationBuilder.DropIndex(
                name: "IX_KycTemplates_SupplierTypeId_KycAttributeDefinitionId",
                table: "KycTemplates");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_UserId_IsDeleted",
                table: "UserDocuments",
                columns: new[] { "UserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_UserId_KycAttributeDefinitionId_IsDeleted",
                table: "UserDocuments",
                columns: new[] { "UserId", "KycAttributeDefinitionId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDocuments_UserId_IsDeleted",
                table: "UserDocuments");

            migrationBuilder.DropIndex(
                name: "IX_UserDocuments_UserId_KycAttributeDefinitionId_IsDeleted",
                table: "UserDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_UserId",
                table: "UserDocuments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KycTemplates_SupplierTypeId_KycAttributeDefinitionId",
                table: "KycTemplates",
                columns: new[] { "SupplierTypeId", "KycAttributeDefinitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KycTemplates_SupplierTypes_SupplierTypeId",
                table: "KycTemplates",
                column: "SupplierTypeId",
                principalTable: "SupplierTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
