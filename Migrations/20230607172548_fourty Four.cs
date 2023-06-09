using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class fourtyFour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "clientSubscriptionId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "commandCenterClientId",
                table: "Users",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "clientSubscriptionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "commandCenterClientId",
                table: "Users");
        }
    }
}
