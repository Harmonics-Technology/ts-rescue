using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimesheetBE.Migrations
{
    public partial class one : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    clientId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    clientSecret = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    concurrencyToken = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    consentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    displayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    displayNames = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    permissions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    postLogoutRedirectUris = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    properties = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    redirectUris = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requirements = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictApplications", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    concurrencyToken = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descriptions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    displayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    displayNames = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    properties = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resources = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictScopes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payRollTypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_payRollTypes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    normalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    concurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_Roles", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_statuses", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    firstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    otherNames = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    profilePicture = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationPhone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    organizationAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    clientId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    isActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    employeeInformationId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    createdById = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    userName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    normalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    normalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    passwordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    securityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    concurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    twoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    lockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    lockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    accessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_Users", x => x.id);
                    table.ForeignKey(
                        name: "fK_Users_Users_clientId1",
                        column: x => x.clientId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_Users_Users_createdById",
                        column: x => x.createdById,
                        principalTable: "Users",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    applicationId = table.Column<int>(type: "int", nullable: true),
                    concurrencyToken = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    properties = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scopes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subject = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictAuthorizations", x => x.id);
                    table.ForeignKey(
                        name: "fK_OpenIddictAuthorizations_OpenIddictApplications_applicationId",
                        column: x => x.applicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    roleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    claimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_RoleClaims", x => x.id);
                    table.ForeignKey(
                        name: "fK_RoleClaims_Roles_roleId",
                        column: x => x.roleId,
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "expenseTypes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_expenseTypes", x => x.id);
                    table.ForeignKey(
                        name: "fK_expenseTypes_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    employeeInformationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    totalHours = table.Column<int>(type: "int", nullable: false),
                    totalAmount = table.Column<double>(type: "double", nullable: false),
                    rate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    paymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fK_invoices_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    codeString = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    key = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    expiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    isExpired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_codes", x => x.id);
                    table.ForeignKey(
                        name: "fK_codes_users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "employeeInformation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    userId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    payRollTypeId = table.Column<int>(type: "int", nullable: false),
                    clientId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    supervisorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ratePerHour = table.Column<double>(type: "double", nullable: false),
                    jobTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoursPerDay = table.Column<int>(type: "int", nullable: false),
                    inCorporationDocumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    voidCheckUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    insuranceDocumentUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hstNumber = table.Column<int>(type: "int", nullable: false),
                    paymentPartnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    paymentRate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    currency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fixedAmount = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    clientRate = table.Column<double>(type: "double", nullable: true),
                    monthlyPayoutRate = table.Column<double>(type: "double", nullable: true),
                    paymentFrequency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_employeeInformation", x => x.id);
                    table.ForeignKey(
                        name: "fK_employeeInformation_payRollTypes_payRollTypeId",
                        column: x => x.payRollTypeId,
                        principalTable: "payRollTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_employeeInformation_Users_clientId1",
                        column: x => x.clientId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_employeeInformation_Users_paymentPartnerId",
                        column: x => x.paymentPartnerId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_employeeInformation_Users_supervisorId1",
                        column: x => x.supervisorId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_employeeInformation_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    claimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_UserClaims", x => x.id);
                    table.ForeignKey(
                        name: "fK_UserClaims_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    loginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    providerKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    providerDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_UserLogins", x => new { x.loginProvider, x.providerKey });
                    table.ForeignKey(
                        name: "fK_UserLogins_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    roleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_UserRoles", x => new { x.userId, x.roleId });
                    table.ForeignKey(
                        name: "fK_UserRoles_Roles_roleId",
                        column: x => x.roleId,
                        principalTable: "Roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_UserRoles_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    loginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_UserTokens", x => new { x.userId, x.loginProvider, x.name });
                    table.ForeignKey(
                        name: "fK_UserTokens_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    applicationId = table.Column<int>(type: "int", nullable: true),
                    authorizationId = table.Column<int>(type: "int", nullable: true),
                    concurrencyToken = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    expirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    payload = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    properties = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    redemptionDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    referenceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subject = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictTokens", x => x.id);
                    table.ForeignKey(
                        name: "fK_OpenIddictTokens_OpenIddictApplications_applicationId",
                        column: x => x.applicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_OpenIddictTokens_OpenIddictAuthorizations_authorizationId",
                        column: x => x.authorizationId,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    teamMemberId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    enponseTypeId = table.Column<int>(type: "int", nullable: false),
                    expenseTypeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    currency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    createdByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fK_expenses_expenseTypes_ExpenseTypeId",
                        column: x => x.expenseTypeId,
                        principalTable: "expenseTypes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fK_expenses_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_expenses_users_createdByUserId",
                        column: x => x.createdByUserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_expenses_users_teamMemberId",
                        column: x => x.teamMemberId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    document = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    employeeInformationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_contracts", x => x.id);
                    table.ForeignKey(
                        name: "fK_contracts_employeeInformation_employeeInformationId",
                        column: x => x.employeeInformationId,
                        principalTable: "employeeInformation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_contracts_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payrolls",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    employeeInformationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    totalHours = table.Column<int>(type: "int", nullable: false),
                    totalAmount = table.Column<double>(type: "double", nullable: false),
                    rate = table.Column<double>(type: "double", nullable: false),
                    paymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    payRollTypeId = table.Column<int>(type: "int", nullable: false),
                    statusId = table.Column<int>(type: "int", nullable: false),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_payrolls", x => x.id);
                    table.ForeignKey(
                        name: "fK_payrolls_employeeInformation_employeeInformationId",
                        column: x => x.employeeInformationId,
                        principalTable: "employeeInformation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_payrolls_payRollTypes_payRollTypeId",
                        column: x => x.payRollTypeId,
                        principalTable: "payRollTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_payrolls_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "timeSheets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    isApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    employeeInformationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    hours = table.Column<int>(type: "int", nullable: false),
                    statusId = table.Column<int>(type: "int", nullable: true),
                    rejectionReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_timeSheets", x => x.id);
                    table.ForeignKey(
                        name: "fK_timeSheets_employeeInformation_employeeInformationId",
                        column: x => x.employeeInformationId,
                        principalTable: "employeeInformation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fK_timeSheets_statuses_statusId",
                        column: x => x.statusId,
                        principalTable: "statuses",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "paySlips",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    employeeInformationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    totalHours = table.Column<int>(type: "int", nullable: false),
                    totalAmount = table.Column<double>(type: "double", nullable: false),
                    rate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    paymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    payrollId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    dateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    dateModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_paySlips", x => x.id);
                    table.ForeignKey(
                        name: "fK_paySlips_payrolls_payrollId",
                        column: x => x.payrollId,
                        principalTable: "payrolls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "iX_codes_userId",
                table: "codes",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "iX_contracts_employeeInformationId",
                table: "contracts",
                column: "employeeInformationId");

            migrationBuilder.CreateIndex(
                name: "iX_contracts_statusId",
                table: "contracts",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_employeeInformation_clientId",
                table: "employeeInformation",
                column: "clientId");

            migrationBuilder.CreateIndex(
                name: "iX_employeeInformation_paymentPartnerId",
                table: "employeeInformation",
                column: "paymentPartnerId");

            migrationBuilder.CreateIndex(
                name: "iX_employeeInformation_payRollTypeId",
                table: "employeeInformation",
                column: "payRollTypeId");

            migrationBuilder.CreateIndex(
                name: "iX_employeeInformation_supervisorId",
                table: "employeeInformation",
                column: "supervisorId");

            migrationBuilder.CreateIndex(
                name: "iX_employeeInformation_userId",
                table: "employeeInformation",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_expenses_createdByUserId",
                table: "expenses",
                column: "createdByUserId");

            migrationBuilder.CreateIndex(
                name: "iX_expenses_expenseTypeId",
                table: "expenses",
                column: "expenseTypeId");

            migrationBuilder.CreateIndex(
                name: "iX_expenses_statusId",
                table: "expenses",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_expenses_teamMemberId",
                table: "expenses",
                column: "teamMemberId");

            migrationBuilder.CreateIndex(
                name: "iX_expenseTypes_statusId",
                table: "expenseTypes",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_invoices_statusId",
                table: "invoices",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictApplications_clientId",
                table: "OpenIddictApplications",
                column: "clientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictAuthorizations_applicationId_status_subject_type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "applicationId", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictScopes_name",
                table: "OpenIddictScopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictTokens_applicationId_status_subject_type",
                table: "OpenIddictTokens",
                columns: new[] { "applicationId", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictTokens_authorizationId",
                table: "OpenIddictTokens",
                column: "authorizationId");

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictTokens_referenceId",
                table: "OpenIddictTokens",
                column: "referenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_payrolls_employeeInformationId",
                table: "payrolls",
                column: "employeeInformationId");

            migrationBuilder.CreateIndex(
                name: "iX_payrolls_payRollTypeId",
                table: "payrolls",
                column: "payRollTypeId");

            migrationBuilder.CreateIndex(
                name: "iX_payrolls_statusId",
                table: "payrolls",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_paySlips_payrollId",
                table: "paySlips",
                column: "payrollId");

            migrationBuilder.CreateIndex(
                name: "iX_RoleClaims_roleId",
                table: "RoleClaims",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "normalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_timeSheets_employeeInformationId",
                table: "timeSheets",
                column: "employeeInformationId");

            migrationBuilder.CreateIndex(
                name: "iX_timeSheets_statusId",
                table: "timeSheets",
                column: "statusId");

            migrationBuilder.CreateIndex(
                name: "iX_UserClaims_userId",
                table: "UserClaims",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "iX_UserLogins_userId",
                table: "UserLogins",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "iX_UserRoles_roleId",
                table: "UserRoles",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "normalizedEmail");

            migrationBuilder.CreateIndex(
                name: "iX_Users_clientId",
                table: "Users",
                column: "clientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_Users_createdById",
                table: "Users",
                column: "createdById");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "normalizedUserName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "codes");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "OpenIddictScopes");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "paySlips");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "timeSheets");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "expenseTypes");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");

            migrationBuilder.DropTable(
                name: "payrolls");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");

            migrationBuilder.DropTable(
                name: "employeeInformation");

            migrationBuilder.DropTable(
                name: "statuses");

            migrationBuilder.DropTable(
                name: "payRollTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
