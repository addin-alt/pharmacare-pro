using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class StockMovement : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MedicineBatchId { get; set; }

    public MedicineBatch? MedicineBatch { get; set; }

    public StockMovementType MovementType { get; set; }

    public int QuantityChange { get; set; }

    [Range(0, 10_000_000)]
    public int BalanceAfter { get; set; }

    [StringLength(80)]
    public string? ReferenceNumber { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (MedicineBatchId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A medicine batch is required.",
                [nameof(MedicineBatchId)]);
        }

        if (QuantityChange == 0)
        {
            yield return new ValidationResult(
                "Stock movement quantity cannot be zero.",
                [nameof(QuantityChange)]);
        }
    }
}
