using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCarePro.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicineBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "integer", nullable: false),
                    FreeQuantity = table.Column<int>(type: "integer", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    PurchaseReference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RackLocation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsQuarantined = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineBatches", x => x.Id);
                    table.CheckConstraint("CK_MedicineBatches_AvailableQuantity", "\"AvailableQuantity\" >= 0");
                    table.CheckConstraint("CK_MedicineBatches_FreeQuantity", "\"FreeQuantity\" >= 0");
                    table.CheckConstraint("CK_MedicineBatches_ReceivedQuantity", "\"ReceivedQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_MedicineBatches_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    QuantityChange = table.Column<int>(type: "integer", nullable: false),
                    BalanceAfter = table.Column<int>(type: "integer", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.CheckConstraint("CK_StockMovements_BalanceAfter", "\"BalanceAfter\" >= 0");
                    table.CheckConstraint("CK_StockMovements_QuantityChange", "\"QuantityChange\" <> 0");
                    table.ForeignKey(
                        name: "FK_StockMovements_MedicineBatches_MedicineBatchId",
                        column: x => x.MedicineBatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_AvailableQuantity",
                table: "MedicineBatches",
                column: "AvailableQuantity");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_ExpiryDate",
                table: "MedicineBatches",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_IsQuarantined",
                table: "MedicineBatches",
                column: "IsQuarantined");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_MedicineId_BatchNumber",
                table: "MedicineBatches",
                columns: new[] { "MedicineId", "BatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CreatedAtUtc",
                table: "StockMovements",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_MedicineBatchId",
                table: "StockMovements",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_MovementType",
                table: "StockMovements",
                column: "MovementType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "MedicineBatches");
        }
    }
}
