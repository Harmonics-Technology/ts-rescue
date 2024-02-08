using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class ninetyeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "annualBilling",
                table: "clientSubscriptionDetails",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "endDate",
                table: "clientSubscriptionDetails",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "paymentMethod",
                table: "clientSubscriptionDetails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "startDate",
                table: "clientSubscriptionDetails",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "totalAmount",
                table: "clientSubscriptionDetails",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "annualBilling",
                table: "clientSubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "endDate",
                table: "clientSubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "paymentMethod",
                table: "clientSubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "startDate",
                table: "clientSubscriptionDetails");

            migrationBuilder.DropColumn(
                name: "totalAmount",
                table: "clientSubscriptionDetails");
        }
    }
}
