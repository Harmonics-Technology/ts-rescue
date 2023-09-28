using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class seventynine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "timesheetFillingReminderDay",
                table: "controlSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "timesheetOverdueReminderDay",
                table: "controlSettings",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timesheetFillingReminderDay",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "timesheetOverdueReminderDay",
                table: "controlSettings");
        }
    }
}
