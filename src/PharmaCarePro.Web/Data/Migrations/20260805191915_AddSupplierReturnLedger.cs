using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCarePro.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierReturnLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupplierReturns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurchaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrossReturnAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PayableReductionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SupplierRefundAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReturnedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierReturns", x => x.Id);
                    table.CheckConstraint("CK_SupplierReturns_GrossReturnAmount", "\"GrossReturnAmount\" >= 0");
                    table.CheckConstraint("CK_SupplierReturns_PayableReductionAmount", "\"PayableReductionAmount\" >= 0");
                    table.CheckConstraint("CK_SupplierReturns_RefundMethod", "(\"SupplierRefundAmount\" = 0 AND \"RefundMethod\" IS NULL) OR (\"SupplierRefundAmount\" > 0 AND \"RefundMethod\" IS NOT NULL AND \"RefundMethod\" <> 'Due')");
                    table.CheckConstraint("CK_SupplierReturns_SettlementTotal", "\"PayableReductionAmount\" + \"SupplierRefundAmount\" = \"GrossReturnAmount\"");
                    table.CheckConstraint("CK_SupplierReturns_SupplierRefundAmount", "\"SupplierRefundAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_SupplierReturns_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierReturns_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierReturnItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierReturnId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    FreeQuantity = table.Column<int>(type: "integer", nullable: false),
                    UnitReturnAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineReturnAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierReturnItems", x => x.Id);
                    table.CheckConstraint("CK_SupplierReturnItems_FreeQuantity", "\"FreeQuantity\" >= 0");
                    table.CheckConstraint("CK_SupplierReturnItems_LineReturnAmount", "\"LineReturnAmount\" >= 0");
                    table.CheckConstraint("CK_SupplierReturnItems_Quantity", "\"Quantity\" >= 0");
                    table.CheckConstraint("CK_SupplierReturnItems_TotalQuantity", "\"Quantity\" + \"FreeQuantity\" > 0");
                    table.CheckConstraint("CK_SupplierReturnItems_UnitReturnAmount", "\"UnitReturnAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_SupplierReturnItems_MedicineBatches_MedicineBatchId",
                        column: x => x.MedicineBatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierReturnItems_PurchaseItems_PurchaseItemId",
                        column: x => x.PurchaseItemId,
                        principalTable: "PurchaseItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierReturnItems_SupplierReturns_SupplierReturnId",
                        column: x => x.SupplierReturnId,
                        principalTable: "SupplierReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturnItems_MedicineBatchId",
                table: "SupplierReturnItems",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturnItems_PurchaseItemId",
                table: "SupplierReturnItems",
                column: "PurchaseItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturnItems_SupplierReturnId_PurchaseItemId",
                table: "SupplierReturnItems",
                columns: new[] { "SupplierReturnId", "PurchaseItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturns_PurchaseId",
                table: "SupplierReturns",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturns_ReturnedAtUtc",
                table: "SupplierReturns",
                column: "ReturnedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturns_ReturnNumber",
                table: "SupplierReturns",
                column: "ReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReturns_SupplierId",
                table: "SupplierReturns",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierReturnItems");

            migrationBuilder.DropTable(
                name: "SupplierReturns");
        }
    }
}
