using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class SupplierReturnItem : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SupplierReturnId { get; set; }

    public SupplierReturn SupplierReturn { get; set; } = null!;

    public Guid PurchaseItemId { get; set; }

    public PurchaseItem PurchaseItem { get; set; } = null!;

    public Guid MedicineBatchId { get; set; }

    public MedicineBatch MedicineBatch { get; set; } = null!;

    [Range(0, 10_000_000)]
    public int Quantity { get; set; }

    [Range(0, 10_000_000)]
    public int FreeQuantity { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal UnitReturnAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal LineReturnAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SupplierReturnId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A supplier return is required.",
                [nameof(SupplierReturnId)]);
        }

        if (PurchaseItemId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A purchased item is required.",
                [nameof(PurchaseItemId)]);
        }

        if (MedicineBatchId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A medicine batch is required.",
                [nameof(MedicineBatchId)]);
        }

        if (Quantity + FreeQuantity <= 0)
        {
            yield return new ValidationResult(
                "At least one paid or free unit must be returned.",
                [nameof(Quantity), nameof(FreeQuantity)]);
        }

        var expectedLineAmount =
            Math.Round(
                Quantity * UnitReturnAmount,
                2,
                MidpointRounding.AwayFromZero);

        if (Math.Abs(
                expectedLineAmount -
                LineReturnAmount) > 0.01m)
        {
            yield return new ValidationResult(
                "The supplier return line amount does not match " +
                "its paid quantity.",
                [
                    nameof(UnitReturnAmount),
                    nameof(LineReturnAmount)
                ]);
        }
    }
}
