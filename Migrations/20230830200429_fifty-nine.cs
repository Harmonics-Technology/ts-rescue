using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fiftynine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTasks_projectTaskId",
                table: "projectSubTasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "projectTaskId",
                table: "projectSubTasks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTasks_projectTaskId",
                table: "projectSubTasks",
                column: "projectTaskId",
                principalTable: "projectTasks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTasks_projectTaskId",
                table: "projectSubTasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "projectTaskId",
                table: "projectSubTasks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTasks_projectTaskId",
                table: "projectSubTasks",
                column: "projectTaskId",
                principalTable: "projectTasks",
                principalColumn: "id");
        }
    }
}
