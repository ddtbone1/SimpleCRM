using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SimpleCRM.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    EmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "TEXT", nullable: true),
                    EmailVerificationSentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AgentId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovalComments = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Timesheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoursWorked = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsBillable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    ApprovedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovalComments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EditJustification = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timesheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timesheets_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Timesheets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimesheetId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Hours = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsBillable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Timesheets_TimesheetId",
                        column: x => x.TimesheetId,
                        principalTable: "Timesheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Agents",
                columns: new[] { "Id", "CreatedDate", "DeletedAt", "Department", "Email", "HireDate", "IsActive", "Name", "Phone", "Position", "UpdatedDate" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Sales", "agent@crm.local", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Agent User", "555-0100", "Agent", null });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "CreatedDate", "DeletedAt", "Email", "IsActive", "Name", "Phone", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer1@example.com", true, "Customer 1", "555-1001", null },
                    { 2, new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer2@example.com", true, "Customer 2", "555-1002", null },
                    { 3, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer3@example.com", true, "Customer 3", "555-1003", null },
                    { 4, new DateTime(2023, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer4@example.com", false, "Customer 4", "555-1004", null },
                    { 5, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer5@example.com", true, "Customer 5", "555-1005", null },
                    { 6, new DateTime(2023, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer6@example.com", true, "Customer 6", "555-1006", null },
                    { 7, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer7@example.com", true, "Customer 7", "555-1007", null },
                    { 8, new DateTime(2023, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer8@example.com", false, "Customer 8", "555-1008", null },
                    { 9, new DateTime(2023, 12, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer9@example.com", true, "Customer 9", "555-1009", null },
                    { 10, new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer10@example.com", true, "Customer 10", "555-1010", null },
                    { 11, new DateTime(2023, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer11@example.com", true, "Customer 11", "555-1011", null },
                    { 12, new DateTime(2023, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer12@example.com", false, "Customer 12", "555-1012", null },
                    { 13, new DateTime(2023, 12, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer13@example.com", true, "Customer 13", "555-1013", null },
                    { 14, new DateTime(2023, 12, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer14@example.com", true, "Customer 14", "555-1014", null },
                    { 15, new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer15@example.com", true, "Customer 15", "555-1015", null },
                    { 16, new DateTime(2023, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer16@example.com", false, "Customer 16", "555-1016", null },
                    { 17, new DateTime(2023, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer17@example.com", true, "Customer 17", "555-1017", null },
                    { 18, new DateTime(2023, 12, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer18@example.com", true, "Customer 18", "555-1018", null },
                    { 19, new DateTime(2023, 12, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer19@example.com", true, "Customer 19", "555-1019", null },
                    { 20, new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "customer20@example.com", false, "Customer 20", "555-1020", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AgentId", "ApprovalComments", "ApprovedByUserId", "ApprovedDate", "CreatedDate", "DeletedAt", "Email", "EmailVerificationSentAt", "EmailVerificationToken", "EmailVerified", "Password", "PasswordResetToken", "PasswordResetTokenExpiry", "Role", "Status", "UpdatedDate", "Username" },
                values: new object[,]
                {
                    { 1, null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@crm.local", null, null, true, "admin123", null, null, "Admin", "ACTIVE", null, "admin" },
                    { 2, 1, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "agent@crm.local", null, null, true, "agent123", null, null, "User", "ACTIVE", null, "agent" }
                });

            migrationBuilder.InsertData(
                table: "Timesheets",
                columns: new[] { "Id", "ApprovalComments", "ApprovedByUserId", "ApprovedDate", "Category", "CreatedDate", "Date", "DeletedAt", "Description", "EditJustification", "EndTime", "HoursWorked", "IsBillable", "ProjectName", "RejectionReason", "StartTime", "Status", "SubmittedDate", "UpdatedDate", "UserId" },
                values: new object[,]
                {
                    { 1, "", null, null, "Development", new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 31", null, new DateTime(2023, 12, 31, 15, 0, 0, 0, DateTimeKind.Unspecified), 6.5, true, "Customer Project", "", new DateTime(2023, 12, 31, 9, 0, 0, 0, DateTimeKind.Unspecified), "APPROVED", null, null, 2 },
                    { 2, "", null, null, "Development", new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 30", null, new DateTime(2023, 12, 30, 16, 0, 0, 0, DateTimeKind.Unspecified), 7.0, true, "Customer Project", "", new DateTime(2023, 12, 30, 9, 0, 0, 0, DateTimeKind.Unspecified), "APPROVED", null, null, 2 },
                    { 3, "", null, null, "Development", new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 29", null, new DateTime(2023, 12, 29, 17, 0, 0, 0, DateTimeKind.Unspecified), 7.5, true, "Customer Project", "", new DateTime(2023, 12, 29, 9, 0, 0, 0, DateTimeKind.Unspecified), "PENDING", null, null, 2 },
                    { 4, "", null, null, "Development", new DateTime(2023, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 28", null, new DateTime(2023, 12, 28, 15, 0, 0, 0, DateTimeKind.Unspecified), 6.0, true, "Customer Project", "", new DateTime(2023, 12, 28, 9, 0, 0, 0, DateTimeKind.Unspecified), "APPROVED", null, null, 2 },
                    { 5, "", null, null, "Development", new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 27", null, new DateTime(2023, 12, 27, 15, 0, 0, 0, DateTimeKind.Unspecified), 6.5, true, "Customer Project", "", new DateTime(2023, 12, 27, 9, 0, 0, 0, DateTimeKind.Unspecified), "APPROVED", null, null, 2 },
                    { 6, "", null, null, "Development", new DateTime(2023, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 26", null, new DateTime(2023, 12, 26, 16, 0, 0, 0, DateTimeKind.Unspecified), 7.0, true, "Customer Project", "", new DateTime(2023, 12, 26, 9, 0, 0, 0, DateTimeKind.Unspecified), "PENDING", null, null, 2 },
                    { 7, "", null, null, "Development", new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 25", null, new DateTime(2023, 12, 25, 17, 0, 0, 0, DateTimeKind.Unspecified), 7.5, true, "Customer Project", "", new DateTime(2023, 12, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), "APPROVED", null, null, 2 },
                    { 8, "", null, null, "Development", new DateTime(2023, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 24", null, new DateTime(2023, 12, 24, 15, 0, 0, 0, DateTimeKind.Unspecified), 6.0, true, "Customer Project", "", new DateTime(2023, 12, 24, 9, 0, 0, 0, DateTimeKind.Unspecified), "APPROVED", null, null, 2 },
                    { 9, "", null, null, "Development", new DateTime(2023, 12, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 23", null, new DateTime(2023, 12, 23, 15, 0, 0, 0, DateTimeKind.Unspecified), 6.5, true, "Customer Project", "", new DateTime(2023, 12, 23, 9, 0, 0, 0, DateTimeKind.Unspecified), "PENDING", null, null, 2 },
                    { 10, "", null, null, "Development", new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Work completed on Dec 22", null, new DateTime(2023, 12, 22, 16, 0, 0, 0, DateTimeKind.Unspecified), 7.0, true, "Customer Project", "", new DateTime(2023, 12, 22, 9, 0, 0, 0, DateTimeKind.Unspecified), "APPROVED", null, null, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedDate",
                table: "AuditLogs",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_Date",
                table: "TimeEntries",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_TimesheetId",
                table: "TimeEntries",
                column: "TimesheetId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId",
                table: "TimeEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_ApprovedByUserId",
                table: "Timesheets",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_Date",
                table: "Timesheets",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_Status",
                table: "Timesheets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_UserId",
                table: "Timesheets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AgentId",
                table: "Users",
                column: "AgentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "TimeEntries");

            migrationBuilder.DropTable(
                name: "Timesheets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Agents");
        }
    }
}
