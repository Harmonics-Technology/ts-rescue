using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class seventyeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reason",
                table: "projectTimesheets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "statusId",
                table: "projectTimesheets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "biWeeklyBeginingPeriodDate",
                table: "controlSettings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "biWeeklyPaymentPeriod",
                table: "controlSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isMonthlyPayScheduleFullMonth",
                table: "controlSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "monthlyPaymentPeriod",
                table: "controlSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "montlyBeginingPeriodDate",
                table: "controlSettings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "weeklyBeginingPeriodDate",
                table: "controlSettings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weeklyPaymentPeriod",
                table: "controlSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "iX_projectTimesheets_statusId",
                table: "projectTimesheets",
                column: "statusId");

            migrationBuilder.AddForeignKey(
                name: "fK_projectTimesheets_statuses_statusId",
                table: "projectTimesheets",
                column: "statusId",
                principalTable: "statuses",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fK_projectTimesheets_statuses_statusId",
                table: "projectTimesheets");

            migrationBuilder.DropIndex(
                name: "iX_projectTimesheets_statusId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "reason",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "statusId",
                table: "projectTimesheets");

            migrationBuilder.DropColumn(
                name: "biWeeklyBeginingPeriodDate",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "biWeeklyPaymentPeriod",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "isMonthlyPayScheduleFullMonth",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "monthlyPaymentPeriod",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "montlyBeginingPeriodDate",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "weeklyBeginingPeriodDate",
                table: "controlSettings");

            migrationBuilder.DropColumn(
                name: "weeklyPaymentPeriod",
                table: "controlSettings");
        }
    }
}
