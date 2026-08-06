using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCarePro.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalDocumentPrefixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerPaymentPrefix",
                table: "PharmacyProfiles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "CPY");

            migrationBuilder.AddColumn<string>(
                name: "SaleReturnPrefix",
                table: "PharmacyProfiles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "SRT");

            migrationBuilder.AddColumn<string>(
                name: "SupplierPaymentPrefix",
                table: "PharmacyProfiles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "SPY");

            migrationBuilder.AddColumn<string>(
                name: "SupplierReturnPrefix",
                table: "PharmacyProfiles",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "PRT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerPaymentPrefix",
                table: "PharmacyProfiles");

            migrationBuilder.DropColumn(
                name: "SaleReturnPrefix",
                table: "PharmacyProfiles");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentPrefix",
                table: "PharmacyProfiles");

            migrationBuilder.DropColumn(
                name: "SupplierReturnPrefix",
                table: "PharmacyProfiles");
        }
    }
}
