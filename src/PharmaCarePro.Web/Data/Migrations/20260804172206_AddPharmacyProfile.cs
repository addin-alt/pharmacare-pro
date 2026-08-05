using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCarePro.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PharmacyProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PharmacyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BranchName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    TaxIdentificationNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CurrencySymbol = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InvoicePrefix = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    PurchasePrefix = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    PrescriptionPrefix = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    ExpiryAlertDays = table.Column<int>(type: "integer", nullable: false),
                    LowStockAlertsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiryAlertsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyProfiles", x => x.Id);
                    table.CheckConstraint("CK_PharmacyProfiles_ExpiryAlertDays", "\"ExpiryAlertDays\" >= 1");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyProfiles_PharmacyName_BranchName",
                table: "PharmacyProfiles",
                columns: new[] { "PharmacyName", "BranchName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmacyProfiles");
        }
    }
}
