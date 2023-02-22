using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class seven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_expenses_invoices_invoiceId1",
                table: "expenses");

            migrationBuilder.DropForeignKey(
                name: "fK_invoices_expenses_expenseId1",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_expenseId1",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "expenseId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "expenseId1",
                table: "invoices");

            migrationBuilder.AddColumn<Guid>(
                name: "invoiceId1",
                table: "expenses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_expenses_invoiceId1",
                table: "expenses",
                column: "invoiceId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fK_expenses_invoices_invoiceId",
                table: "expenses",
                column: "invoiceId",
                principalTable: "invoices",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_expenses_invoices_invoiceId1",
                table: "expenses",
                column: "invoiceId1",
                principalTable: "invoices",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_expenses_invoices_invoiceId",
                table: "expenses");

            migrationBuilder.DropForeignKey(
                name: "fK_expenses_invoices_invoiceId1",
                table: "expenses");

            migrationBuilder.DropIndex(
                name: "iX_expenses_invoiceId1",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "invoiceId1",
                table: "expenses");

            migrationBuilder.AddColumn<Guid>(
                name: "expenseId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "expenseId1",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_expenseId1",
                table: "invoices",
                column: "expenseId1");

            migrationBuilder.AddForeignKey(
                name: "fK_expenses_invoices_invoiceId1",
                table: "expenses",
                column: "invoiceId",
                principalTable: "invoices",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_expenses_expenseId1",
                table: "invoices",
                column: "expenseId1",
                principalTable: "expenses",
                principalColumn: "id");
        }
    }
}
