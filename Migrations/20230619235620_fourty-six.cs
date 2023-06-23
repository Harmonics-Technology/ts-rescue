using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fourtysix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_invoices_payrollGroups_payrollGroupId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_payrollGroupId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "payrollGroupId",
                table: "invoices");

            migrationBuilder.AddColumn<Guid>(
                name: "clientId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_clientId",
                table: "invoices",
                column: "clientId");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_users_clientId",
                table: "invoices",
                column: "clientId",
                principalTable: "Users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_invoices_users_clientId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_clientId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "clientId",
                table: "invoices");

            migrationBuilder.AddColumn<int>(
                name: "payrollGroupId",
                table: "invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "iX_invoices_payrollGroupId",
                table: "invoices",
                column: "payrollGroupId");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_payrollGroups_payrollGroupId",
                table: "invoices",
                column: "payrollGroupId",
                principalTable: "payrollGroups",
                principalColumn: "id");
        }
    }
}
