using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionHistoryAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisitorCommissionRule_Visitor_Code_Unique",
                table: "VisitorCommissionRules");

            migrationBuilder.AddColumn<int>(
                name: "CommissionRuleId",
                table: "CommissionTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisitorCommissionRuleId",
                table: "CommissionTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 616, DateTimeKind.Utc).AddTicks(2483));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 616, DateTimeKind.Utc).AddTicks(2486));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3983));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3986));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3988));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(3990));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6319), new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6315) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6321), new DateTime(2026, 1, 28, 9, 0, 12, 621, DateTimeKind.Utc).AddTicks(6321) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 28, 9, 0, 12, 631, DateTimeKind.Utc).AddTicks(6053));

            migrationBuilder.CreateIndex(
                name: "IX_VisitorCommissionRule_Visitor_Code_Dates",
                table: "VisitorCommissionRules",
                columns: new[] { "VisitorProfileId", "CommissionRuleCode", "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_CommissionRuleId",
                table: "CommissionTransactions",
                column: "CommissionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_VisitorCommissionRuleId",
                table: "CommissionTransactions",
                column: "VisitorCommissionRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionTransactions_CommissionRules_CommissionRuleId",
                table: "CommissionTransactions",
                column: "CommissionRuleId",
                principalTable: "CommissionRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissionTransactions_VisitorCommissionRules_VisitorCommissionRuleId",
                table: "CommissionTransactions",
                column: "VisitorCommissionRuleId",
                principalTable: "VisitorCommissionRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommissionTransactions_CommissionRules_CommissionRuleId",
                table: "CommissionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissionTransactions_VisitorCommissionRules_VisitorCommissionRuleId",
                table: "CommissionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_VisitorCommissionRule_Visitor_Code_Dates",
                table: "VisitorCommissionRules");

            migrationBuilder.DropIndex(
                name: "IX_CommissionTransactions_CommissionRuleId",
                table: "CommissionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CommissionTransactions_VisitorCommissionRuleId",
                table: "CommissionTransactions");

            migrationBuilder.DropColumn(
                name: "CommissionRuleId",
                table: "CommissionTransactions");

            migrationBuilder.DropColumn(
                name: "VisitorCommissionRuleId",
                table: "CommissionTransactions");

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 899, DateTimeKind.Utc).AddTicks(2823));

            migrationBuilder.UpdateData(
                table: "CommissionRules",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 899, DateTimeKind.Utc).AddTicks(2825));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(500));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(502));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(504));

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(506));

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2824), new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2819) });

            migrationBuilder.UpdateData(
                table: "Pricings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EffectiveFrom" },
                values: new object[] { new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2826), new DateTime(2026, 1, 27, 23, 17, 57, 902, DateTimeKind.Utc).AddTicks(2826) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 23, 17, 57, 915, DateTimeKind.Utc).AddTicks(1750));

            migrationBuilder.CreateIndex(
                name: "IX_VisitorCommissionRule_Visitor_Code_Unique",
                table: "VisitorCommissionRules",
                columns: new[] { "VisitorProfileId", "CommissionRuleCode" },
                unique: true);
        }
    }
}
