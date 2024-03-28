using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fourtyfour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "iX_Users_createdById",
            //    table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "clientSubscriptionId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "commandCenterClientId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "superAdminId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "invoiceGenerationType",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateIndex(
            //    name: "iX_Users_createdById",
            //    table: "Users",
            //    column: "createdById",
            //    unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_Users_superAdminId",
                table: "Users",
                column: "superAdminId",
                unique: true);

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
                name: "fK_Users_Users_superAdminId",
                table: "Users");

            //migrationBuilder.DropIndex(
            //    name: "iX_Users_createdById",
            //    table: "Users");

            migrationBuilder.DropIndex(
                name: "iX_Users_superAdminId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "clientSubscriptionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "commandCenterClientId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "superAdminId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "invoiceGenerationType",
                table: "employeeInformation");

            migrationBuilder.CreateIndex(
                name: "iX_Users_createdById",
                table: "Users",
                column: "createdById");
        }
    }
}
