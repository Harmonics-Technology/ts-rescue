using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class sixtysix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_projectSubTasks_subTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropTable(
                name: "projectSubTasks");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_subTaskId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "subTaskId",
                table: "projectTimesheets");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "subTaskId",
                table: "projectTimesheets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "projectSubTasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    projectTaskAsigneeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    projectTaskId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    durationInHours = table.Column<double>(type: "double", nullable: true),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    isCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    taskPriority = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trackedByHours = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_projectSubTasks", x => x.id);
                    table.ForeignKey(
                        name: "fK_projectSubTasks_projectTaskAsignees_projectTaskAsigneeId",
                        column: x => x.projectTaskAsigneeId,
                        principalTable: "projectTaskAsignees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_projectSubTasks_projectTasks_projectTaskId",
                        column: x => x.projectTaskId,
                        principalTable: "projectTasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_subTaskId",
                table: "projectTimesheets",
                column: "subTaskId");

            migrationBuilder.CreateIndex(
                name: "iX_projectSubTasks_projectTaskAsigneeId",
                table: "projectSubTasks",
                column: "projectTaskAsigneeId");

            migrationBuilder.CreateIndex(
                name: "iX_projectSubTasks_projectTaskId",
                table: "projectSubTasks",
                column: "projectTaskId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_projectSubTasks_subTaskId",
                table: "projectTimesheets",
                column: "subTaskId",
                principalTable: "projectSubTasks",
                principalColumn: "id");
        }
    }
}
