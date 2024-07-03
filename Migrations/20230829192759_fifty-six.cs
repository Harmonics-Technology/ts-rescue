using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftysix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "taskId",
                table: "projectTaskAsignees",
                newName: "projectTaskId");

            migrationBuilder.CreateIndex(
                name: "iX_projectTaskAsignees_projectId",
                table: "projectTaskAsignees",
                column: "projectId");

            migrationBuilder.CreateIndex(
                name: "iX_projectTaskAsignees_projectTaskId",
                table: "projectTaskAsignees",
                column: "projectTaskId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTaskAsignees_projects_projectId",
                table: "projectTaskAsignees",
                column: "projectId",
                principalTable: "projects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTaskAsignees_projectTasks_projectTaskId",
                table: "projectTaskAsignees",
                column: "projectTaskId",
                principalTable: "projectTasks",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectTaskAsignees_projects_projectId",
                table: "projectTaskAsignees");

            migrationBuilder.DropForeignKey(
                name: "fK_projectTaskAsignees_projectTasks_projectTaskId",
                table: "projectTaskAsignees");

            migrationBuilder.DropIndex(
                name: "iX_projectTaskAsignees_projectId",
                table: "projectTaskAsignees");

            migrationBuilder.DropIndex(
                name: "iX_projectTaskAsignees_projectTaskId",
                table: "projectTaskAsignees");

            migrationBuilder.RenameColumn(
                name: "projectTaskId",
                table: "projectTaskAsignees",
                newName: "taskId");
        }
    }
}
