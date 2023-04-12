using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class thirtytwo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "leaveTypes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    leaveTypeIcon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_leaveTypes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "leaves",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    employeeInformationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    leaveTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    reasonForLeave = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    workAssigneeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_leaves", x => x.id);
                    table.ForeignKey(
                        name: "fK_leaves_employeeInformation_employeeInformationId",
                        column: x => x.employeeInformationId,
                        principalTable: "employeeInformation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_leaves_leaveTypes_leaveTypeId",
                        column: x => x.leaveTypeId,
                        principalTable: "leaveTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_leaves_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_leaves_users_workAssigneeId",
                        column: x => x.workAssigneeId,
                        principalTable: "Users",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "iX_leaves_employeeInformationId",
                table: "leaves",
                column: "employeeInformationId");

            migrationBuilder.CreateIndex(
                name: "iX_leaves_leaveTypeId",
                table: "leaves",
                column: "leaveTypeId");

            migrationBuilder.CreateIndex(
                name: "iX_leaves_statusId",
                table: "leaves",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_leaves_workAssigneeId",
                table: "leaves",
                column: "workAssigneeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leaves");

            migrationBuilder.DropTable(
                name: "leaveTypes");
        }
    }
}
