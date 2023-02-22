using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class twelve : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "clientId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "parentId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "paymentPartnerId",
                table: "invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_paySlips_employeeInformationId",
                table: "paySlips",
                column: "employeeInformationId");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_clientId",
                table: "invoices",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_parentId",
                table: "invoices",
                column: "parentId");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_paymentPartnerId",
                table: "invoices",
                column: "paymentPartnerId");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_invoices_parentId",
                table: "invoices",
                column: "parentId",
                principalTable: "invoices",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_users_clientId",
                table: "invoices",
                column: "clientId",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_invoices_users_paymentPartnerId",
                table: "invoices",
                column: "paymentPartnerId",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_paySlips_employeeInformation_employeeInformationId",
                table: "paySlips",
                column: "employeeInformationId",
                principalTable: "employeeInformation",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_invoices_invoices_parentId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "fK_invoices_users_clientId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "fK_invoices_users_paymentPartnerId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "fK_paySlips_employeeInformation_employeeInformationId",
                table: "paySlips");

            migrationBuilder.DropIndex(
                name: "iX_paySlips_employeeInformationId",
                table: "paySlips");

            migrationBuilder.DropIndex(
                name: "iX_invoices_clientId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_parentId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "iX_invoices_paymentPartnerId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "clientId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "parentId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "paymentPartnerId",
                table: "invoices");
        }
    }
}
