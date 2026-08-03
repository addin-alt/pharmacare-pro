using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class MedicineBatch : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MedicineId { get; set; }

    public Medicine? Medicine { get; set; }

    [Required(ErrorMessage = "Batch number is required.")]
    [StringLength(80)]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ManufacturingDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    [Range(1, 10_000_000)]
    public int ReceivedQuantity { get; set; }

    [Range(0, 10_000_000)]
    public int FreeQuantity { get; set; }

    [Range(0, 10_000_000)]
    public int AvailableQuantity { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal SellingPrice { get; set; }

    [StringLength(120)]
    public string? SupplierName { get; set; }

    [StringLength(80)]
    public string? PurchaseReference { get; set; }

    [StringLength(50)]
    public string? RackLocation { get; set; }

    public bool IsQuarantined { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<StockMovement> StockMovements { get; set; } =
        new List<StockMovement>();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (MedicineId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A medicine must be selected.",
                [nameof(MedicineId)]);
        }

        if (ExpiryDate == default)
        {
            yield return new ValidationResult(
                "Expiry date is required.",
                [nameof(ExpiryDate)]);
        }

        if (ManufacturingDate.HasValue &&
            ExpiryDate.Date <= ManufacturingDate.Value.Date)
        {
            yield return new ValidationResult(
                "Expiry date must be after the manufacturing date.",
                [nameof(ExpiryDate), nameof(ManufacturingDate)]);
        }

        if (AvailableQuantity >
            ReceivedQuantity + FreeQuantity)
        {
            yield return new ValidationResult(
                "Available quantity cannot exceed received and free quantity.",
                [nameof(AvailableQuantity)]);
        }
    }
}
