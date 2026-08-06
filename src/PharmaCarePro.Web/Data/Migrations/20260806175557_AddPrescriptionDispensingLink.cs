using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCarePro.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionDispensingLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionId",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionItemId",
                table: "SaleItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_PrescriptionId",
                table: "Sales",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_PrescriptionItemId",
                table: "SaleItems",
                column: "PrescriptionItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_PrescriptionItems_PrescriptionItemId",
                table: "SaleItems",
                column: "PrescriptionItemId",
                principalTable: "PrescriptionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Prescriptions_PrescriptionId",
                table: "Sales",
                column: "PrescriptionId",
                principalTable: "Prescriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_PrescriptionItems_PrescriptionItemId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Prescriptions_PrescriptionId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_PrescriptionId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_PrescriptionItemId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PrescriptionItemId",
                table: "SaleItems");
        }
    }
}
