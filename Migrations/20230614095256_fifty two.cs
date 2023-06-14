using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftytwo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_Users_Users_createdById",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "iX_Users_createdById",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "createdById",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "createdById",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "iX_Users_createdById",
                table: "Users",
                column: "createdById",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fK_Users_Users_createdById",
                table: "Users",
                column: "createdById",
                principalTable: "Users",
                principalColumn: "id");
        }
    }
}
