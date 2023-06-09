using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fourtyfive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_employeeInformation_Users_paymentPartnerId",
                table: "employeeInformation");

            migrationBuilder.DropForeignKey(
                name: "fK_Users_Users_createdById",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "createdById",
                table: "Users",
                newName: "superAdminId");

            migrationBuilder.RenameIndex(
                name: "iX_Users_createdById",
                table: "Users",
                newName: "iX_Users_superAdminId");

            migrationBuilder.AddColumn<Guid>(
                name: "superAdminId",
                table: "employeeInformation",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "superAdminId1",
                table: "employeeInformation",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_employeeInformation_superAdminId1",
                table: "employeeInformation",
                column: "superAdminId1");

            migrationBuilder.AddForeignKey(
                name: "fK_employeeInformation_Users_paymentPartnerId1",
                table: "employeeInformation",
                column: "paymentPartnerId",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_employeeInformation_Users_superAdminId1",
                table: "employeeInformation",
                column: "superAdminId1",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_Users_Users_superAdminId",
                table: "Users",
                column: "superAdminId",
                principalTable: "Users",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_employeeInformation_Users_paymentPartnerId1",
                table: "employeeInformation");

            migrationBuilder.DropForeignKey(
                name: "fK_employeeInformation_Users_superAdminId1",
                table: "employeeInformation");

            migrationBuilder.DropForeignKey(
                name: "fK_Users_Users_superAdminId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "iX_employeeInformation_superAdminId1",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "superAdminId",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "superAdminId1",
                table: "employeeInformation");

            migrationBuilder.RenameColumn(
                name: "superAdminId",
                table: "Users",
                newName: "createdById");

            migrationBuilder.RenameIndex(
                name: "iX_Users_superAdminId",
                table: "Users",
                newName: "iX_Users_createdById");

            migrationBuilder.AddForeignKey(
                name: "fK_employeeInformation_Users_paymentPartnerId",
                table: "employeeInformation",
                column: "paymentPartnerId",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_Users_Users_createdById",
                table: "Users",
                column: "createdById",
                principalTable: "Users",
                principalColumn: "id");
        }
    }
}
