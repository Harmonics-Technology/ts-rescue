using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class sixteen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "onBoradingFee",
                table: "employeeInformation",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "payrollGroupId",
                table: "employeeInformation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payrollGroups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_payrollGroups", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_employeeInformation_payrollGroups_payrollGroupId",
                table: "employeeInformation");

            migrationBuilder.DropTable(
                name: "payrollGroups");

            migrationBuilder.DropIndex(
                name: "iX_employeeInformation_payrollGroupId",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "onBoradingFee",
                table: "employeeInformation");

            migrationBuilder.DropColumn(
                name: "payrollGroupId",
                table: "employeeInformation");
        }
    }
}
