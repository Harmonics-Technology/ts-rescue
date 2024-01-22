using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class ninetysix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "userDrafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    firstName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    clientId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    role = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationPhone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invoiceGenerationFrequency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    term = table.Column<int>(type: "int", nullable: true),
                    clientSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    commandCenterClientId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    superAdminId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    payRollTypeId = table.Column<int>(type: "int", nullable: true),
                    payrollGroupId = table.Column<int>(type: "int", nullable: true),
                    supervisorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ratePerHour = table.Column<double>(type: "double", nullable: true),
                    jobTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoursPerDay = table.Column<int>(type: "int", nullable: true),
                    inCorporationDocumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    voidCheckUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    insuranceDocumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hstNumber = table.Column<int>(type: "int", nullable: true),
                    paymentPartnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    paymentRate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    currency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fixedAmount = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    document = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    clientRate = table.Column<double>(type: "double", nullable: true),
                    monthlyPayoutRate = table.Column<double>(type: "double", nullable: true),
                    paymentFrequency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    isActive = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    onBoradingFee = table.Column<double>(type: "double", nullable: true),
                    isEligibleForLeave = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    numberOfDaysEligible = table.Column<int>(type: "int", nullable: true),
                    numberOfHoursEligible = table.Column<int>(type: "int", nullable: true),
                    employeeType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invoiceGenerationType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    enableFinancials = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_userDrafts", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userDrafts");
        }
    }
}
