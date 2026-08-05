using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class PurchaseItem : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PurchaseId { get; set; }

    public Purchase? Purchase { get; set; }

    public Guid MedicineId { get; set; }

    public Medicine? Medicine { get; set; }

    [Required]
    [StringLength(120)]
    public string MedicineName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string BatchNumber { get; set; } = string.Empty;

    public DateTime? ManufacturingDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    [Range(1, 10_000_000)]
    public int Quantity { get; set; }

    [Range(0, 10_000_000)]
    public int FreeQuantity { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal SellingPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal LineTotal { get; set; }

    [StringLength(50)]
    public string? RackLocation { get; set; }

    public ICollection<SupplierReturnItem> ReturnItems { get; set; } =
        new List<SupplierReturnItem>();

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
    }
}
