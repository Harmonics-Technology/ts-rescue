using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class onehundredtwelve : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "newPayrollStructureEnabled",
                table: "employeeInformation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "newPayrollStructureEnabled",
                table: "employeeInformation",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
