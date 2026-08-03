using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class Sale : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [StringLength(120)]
    public string? CustomerName { get; set; }

    [StringLength(30)]
    public string? CustomerPhone { get; set; }

    public DateTime SoldAtUtc { get; set; } = DateTime.UtcNow;

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

    public PaymentMethod PaymentMethod { get; set; }

    public SaleStatus Status { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SaleItem> Items { get; set; } =
        new List<SaleItem>();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
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

        if (Math.Abs((GrandTotal - PaidAmount) - DueAmount) > 0.01m)
        {
            yield return new ValidationResult(
                "The due amount does not match the sale total.",
                [nameof(DueAmount)]);
        }
    }
}
