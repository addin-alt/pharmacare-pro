using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCarePro.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImmutableSaleItemCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "SaleItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.Sql(
                """
                UPDATE "SaleItems" AS sale_item
                SET "UnitCost" = batch."PurchasePrice"
                FROM "MedicineBatches" AS batch
                WHERE sale_item."MedicineBatchId" = batch."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "SaleItems");
        }
    }
}
