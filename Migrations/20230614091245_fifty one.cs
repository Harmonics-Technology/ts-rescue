using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftyone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoiceGenerationTypeForTeamMember",
                table: "employeeInformation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoiceGenerationTypeForTeamMember",
                table: "employeeInformation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
