using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class six : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_invoices_expenses_expenseId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_expenseId",
                table: "invoices");

            migrationBuilder.AddColumn<Guid>(
                name: "invoiceId",
                table: "payrolls",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "expenseId1",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "invoiceId",
                table: "expenses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_payrolls_invoiceId",
                table: "payrolls",
                column: "invoiceId");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_expenseId1",
                table: "invoices",
                column: "expenseId1");

            migrationBuilder.CreateIndex(
                name: "iX_expenses_invoiceId",
                table: "expenses",
                column: "invoiceId");

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

            migrationBuilder.AddForeignKey(
                name: "fK_payrolls_invoices_invoiceId",
                table: "payrolls",
                column: "invoiceId",
                principalTable: "invoices",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_expenses_invoices_invoiceId1",
                table: "expenses");

            migrationBuilder.DropForeignKey(
                name: "fK_invoices_expenses_expenseId1",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "fK_payrolls_invoices_invoiceId",
                table: "payrolls");

            migrationBuilder.DropIndex(
                name: "iX_payrolls_invoiceId",
                table: "payrolls");

            migrationBuilder.DropIndex(
                name: "iX_invoices_expenseId1",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_expenses_invoiceId",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "invoiceId",
                table: "payrolls");

            migrationBuilder.DropColumn(
                name: "expenseId1",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "invoiceId",
                table: "expenses");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_expenseId",
                table: "invoices",
                column: "expenseId");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_expenses_expenseId",
                table: "invoices",
                column: "expenseId",
                principalTable: "expenses",
                principalColumn: "id");
        }
    }
}
