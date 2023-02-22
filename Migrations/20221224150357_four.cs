using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class four : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "expenseId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "invoiceTypeId",
                table: "invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "invoiceTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_invoiceTypes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_employeeInformationId",
                table: "invoices",
                column: "employeeInformationId");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_expenseId",
                table: "invoices",
                column: "expenseId");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_invoiceTypeId",
                table: "invoices",
                column: "invoiceTypeId");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_employeeInformation_employeeInformationId",
                table: "invoices",
                column: "employeeInformationId",
                principalTable: "employeeInformation",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_expenses_expenseId",
                table: "invoices",
                column: "expenseId",
                principalTable: "expenses",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_invoiceTypes_invoiceTypeId",
                table: "invoices",
                column: "invoiceTypeId",
                principalTable: "invoiceTypes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_invoices_employeeInformation_employeeInformationId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "fK_invoices_expenses_expenseId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "fK_invoices_invoiceTypes_invoiceTypeId",
                table: "invoices");

            migrationBuilder.DropTable(
                name: "invoiceTypes");

            migrationBuilder.DropIndex(
                name: "iX_invoices_employeeInformationId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_expenseId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_invoiceTypeId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "expenseId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "invoiceTypeId",
                table: "invoices");
        }
    }
}
