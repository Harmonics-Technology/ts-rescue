using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class thirteen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_paySlips_employeeInformation_employeeInformationId",
                table: "paySlips");

            migrationBuilder.DropForeignKey(
                name: "fK_paySlips_payrolls_payrollId",
                table: "paySlips");

            migrationBuilder.RenameColumn(
                name: "payrollId",
                table: "paySlips",
                newName: "invoiceId");

            migrationBuilder.RenameIndex(
                name: "iX_paySlips_payrollId",
                table: "paySlips",
                newName: "iX_paySlips_invoiceId");

            migrationBuilder.AlterColumn<Guid>(
                name: "employeeInformationId",
                table: "paySlips",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "fK_paySlips_employeeInformation_employeeInformationId",
                table: "paySlips",
                column: "employeeInformationId",
                principalTable: "employeeInformation",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_paySlips_invoices_invoiceId",
                table: "paySlips",
                column: "invoiceId",
                principalTable: "invoices",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_paySlips_employeeInformation_employeeInformationId",
                table: "paySlips");

            migrationBuilder.DropForeignKey(
                name: "fK_paySlips_invoices_invoiceId",
                table: "paySlips");

            migrationBuilder.RenameColumn(
                name: "invoiceId",
                table: "paySlips",
                newName: "payrollId");

            migrationBuilder.RenameIndex(
                name: "iX_paySlips_invoiceId",
                table: "paySlips",
                newName: "iX_paySlips_payrollId");

            migrationBuilder.AlterColumn<Guid>(
                name: "employeeInformationId",
                table: "paySlips",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "fK_paySlips_employeeInformation_employeeInformationId",
                table: "paySlips",
                column: "employeeInformationId",
                principalTable: "employeeInformation",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fK_paySlips_payrolls_payrollId",
                table: "paySlips",
                column: "payrollId",
                principalTable: "payrolls",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
