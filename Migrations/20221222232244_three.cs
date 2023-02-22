using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class three : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "paymentPartnerId",
                table: "payrolls",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_payrolls_paymentPartnerId",
                table: "payrolls",
                column: "paymentPartnerId");

            migrationBuilder.AddForeignKey(
                name: "fK_payrolls_users_paymentPartnerId",
                table: "payrolls",
                column: "paymentPartnerId",
                principalTable: "Users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_payrolls_users_paymentPartnerId",
                table: "payrolls");

            migrationBuilder.DropIndex(
                name: "iX_payrolls_paymentPartnerId",
                table: "payrolls");

            migrationBuilder.DropColumn(
                name: "paymentPartnerId",
                table: "payrolls");
        }
    }
}
