using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class twentyfive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "clientInvoiceId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_clientInvoiceId",
                table: "invoices",
                column: "clientInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_invoices_clientInvoiceId",
                table: "invoices",
                column: "clientInvoiceId",
                principalTable: "invoices",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_invoices_invoices_clientInvoiceId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_clientInvoiceId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "clientInvoiceId",
                table: "invoices");
        }
    }
}
