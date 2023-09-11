using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class sixtyfive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTaskAsignees_ProjectTaskAsigneeId",
                table: "projectSubTasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "projectTaskAsigneeId",
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
                name: "fK_projectSubTasks_projectTaskAsignees_projectTaskAsigneeId",
                table: "projectSubTasks",
                column: "projectTaskAsigneeId",
                principalTable: "projectTaskAsignees",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectSubTasks_projectTaskAsignees_projectTaskAsigneeId",
                table: "projectSubTasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "projectTaskAsigneeId",
                table: "projectSubTasks",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "fK_projectSubTasks_projectTaskAsignees_ProjectTaskAsigneeId",
                table: "projectSubTasks",
                column: "projectTaskAsigneeId",
                principalTable: "projectTaskAsignees",
                principalColumn: "id");
        }
    }
}
