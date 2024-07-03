using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class eightyone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "percentageOfCompletion",
                table: "projectTasks",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "percentageOfCompletion",
                table: "projectSubTasks",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "percentageOfCompletion",
                table: "projectTasks");

            migrationBuilder.DropColumn(
                name: "percentageOfCompletion",
                table: "projectSubTasks");
        }
    }
}
