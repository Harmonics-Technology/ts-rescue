using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fourtyfive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_employeeInformation_payrollGroups_payrollGroupId",
                table: "employeeInformation");

            migrationBuilder.DropIndex(
                name: "iX_employeeInformation_payrollGroupId",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "payrollGroupId",
                table: "employeeInformation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "payrollGroupId",
                table: "employeeInformation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "iX_employeeInformation_payrollGroupId",
                table: "employeeInformation",
                column: "payrollGroupId");

            migrationBuilder.AddForeignKey(
                name: "fK_employeeInformation_payrollGroups_payrollGroupId",
                table: "employeeInformation",
                column: "payrollGroupId",
                principalTable: "payrollGroups",
                principalColumn: "id");
        }
    }
}
