using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class Purchase : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string PurchaseNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    [StringLength(80)]
    public string? SupplierInvoiceNumber { get; set; }

    public DateTime PurchaseDateUtc { get; set; } = DateTime.UtcNow;

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Subtotal { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal DiscountAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal TaxAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal GrandTotal { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal PaidAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal DueAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } =
        PaymentMethod.Cash;

    public PurchaseStatus Status { get; set; } =
        PurchaseStatus.Draft;

    [StringLength(300)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseItem> Items { get; set; } =
        new List<PurchaseItem>();

    public ICollection<SupplierReturn> Returns { get; set; } =
        new List<SupplierReturn>();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SupplierId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A supplier must be selected.",
                [nameof(SupplierId)]);
        }

        if (DiscountAmount > Subtotal)
        {
            yield return new ValidationResult(
                "Discount cannot exceed the subtotal.",
                [nameof(DiscountAmount)]);
        }

        if (PaidAmount > GrandTotal)
        {
            yield return new ValidationResult(
                "Paid amount cannot exceed the grand total.",
                [nameof(PaidAmount)]);
        }

        var expectedDue = GrandTotal - PaidAmount;

        if (Math.Abs(expectedDue - DueAmount) > 0.01m)
        {
            yield return new ValidationResult(
                "The due amount does not match the purchase total.",
                [nameof(DueAmount)]);
        }
    }
}
