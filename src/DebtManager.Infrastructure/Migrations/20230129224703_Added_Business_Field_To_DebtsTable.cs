using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebtManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedBusinessFieldToDebtsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId",
                table: "Debt",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Debt_BusinessId",
                table: "Debt",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Debt_Business_BusinessId",
                table: "Debt",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Debt_Business_BusinessId",
                table: "Debt");

            migrationBuilder.DropIndex(
                name: "IX_Debt_BusinessId",
                table: "Debt");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Debt");
        }
    }
}
