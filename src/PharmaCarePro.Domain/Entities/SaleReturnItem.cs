using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class SaleReturnItem : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SaleReturnId { get; set; }

    public SaleReturn SaleReturn { get; set; } = null!;

    public Guid SaleItemId { get; set; }

    public SaleItem SaleItem { get; set; } = null!;

    public Guid MedicineBatchId { get; set; }

    public MedicineBatch MedicineBatch { get; set; } = null!;

    [Range(1, 10_000_000)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal UnitRefundAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal LineRefundAmount { get; set; }

    public ReturnStockAction StockAction { get; set; } =
        ReturnStockAction.Quarantine;

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SaleReturnId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A sale return is required.",
                [nameof(SaleReturnId)]);
        }

        if (SaleItemId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A sold item is required.",
                [nameof(SaleItemId)]);
        }

        if (MedicineBatchId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A medicine batch is required.",
                [nameof(MedicineBatchId)]);
        }

        var expectedLineAmount =
            Math.Round(
                UnitRefundAmount * Quantity,
                2,
                MidpointRounding.AwayFromZero);

        if (Math.Abs(
                expectedLineAmount - LineRefundAmount) >
            0.01m)
        {
            yield return new ValidationResult(
                "The return line amount does not match its quantity.",
                [
                    nameof(UnitRefundAmount),
                    nameof(LineRefundAmount)
                ]);
        }
    }
}
