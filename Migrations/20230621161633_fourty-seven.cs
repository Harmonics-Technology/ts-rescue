using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fourtyseven : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "iX_Users_superAdminId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "iX_Users_superAdminId",
                table: "Users",
                column: "superAdminId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "iX_Users_superAdminId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "iX_Users_superAdminId",
                table: "Users",
                column: "superAdminId",
                unique: true);
        }
    }
}
