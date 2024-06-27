using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class onehundredfifteen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "category",
                table: "projectTasks",
                newName: "operationalTaskStatus");

            migrationBuilder.AlterColumn<bool>(
                name: "trackedByHours",
                table: "projectTasks",
                type: "tinyint(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<double>(
                name: "durationInHours",
                table: "projectTasks",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddColumn<bool>(
                name: "isAssignedToMe",
                table: "projectTasks",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isOperationalTask",
                table: "projectTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isAssignedToMe",
                table: "projectTasks");

            migrationBuilder.DropColumn(
                name: "isOperationalTask",
                table: "projectTasks");

            migrationBuilder.RenameColumn(
                name: "operationalTaskStatus",
                table: "projectTasks",
                newName: "category");

            migrationBuilder.AlterColumn<bool>(
                name: "trackedByHours",
                table: "projectTasks",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "durationInHours",
                table: "projectTasks",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);
        }
    }
}
