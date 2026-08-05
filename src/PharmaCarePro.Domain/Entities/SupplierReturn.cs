using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class SupplierReturn : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string ReturnNumber { get; set; } = string.Empty;

    public Guid PurchaseId { get; set; }

    public Purchase Purchase { get; set; } = null!;

    public Guid SupplierId { get; set; }

    public Supplier Supplier { get; set; } = null!;

    [Range(typeof(decimal), "0", "999999999")]
    public decimal GrossReturnAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal PayableReductionAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal SupplierRefundAmount { get; set; }

    public PaymentMethod? RefundMethod { get; set; }

    [Required]
    [StringLength(200)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    [StringLength(200)]
    public string RecordedBy { get; set; } = string.Empty;

    public DateTime ReturnedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public ICollection<SupplierReturnItem> Items { get; set; } =
        new List<SupplierReturnItem>();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (PurchaseId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A purchase is required.",
                [nameof(PurchaseId)]);
        }

        if (SupplierId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A supplier is required.",
                [nameof(SupplierId)]);
        }

        var settlementTotal =
            Math.Round(
                PayableReductionAmount +
                SupplierRefundAmount,
                2,
                MidpointRounding.AwayFromZero);

        var returnTotal =
            Math.Round(
                GrossReturnAmount,
                2,
                MidpointRounding.AwayFromZero);

        if (Math.Abs(settlementTotal - returnTotal) > 0.01m)
        {
            yield return new ValidationResult(
                "The payable reduction and supplier refund must " +
                "equal the return total.",
                [
                    nameof(PayableReductionAmount),
                    nameof(SupplierRefundAmount)
                ]);
        }

        if (SupplierRefundAmount > 0 &&
            RefundMethod is null)
        {
            yield return new ValidationResult(
                "A refund method is required when the supplier " +
                "returns money.",
                [nameof(RefundMethod)]);
        }

        if (RefundMethod == PaymentMethod.Due)
        {
            yield return new ValidationResult(
                "Due is not a valid supplier refund method.",
                [nameof(RefundMethod)]);
        }

        if (SupplierRefundAmount == 0 &&
            RefundMethod is not null)
        {
            yield return new ValidationResult(
                "A refund method must not be selected when no " +
                "money is refunded.",
                [nameof(RefundMethod)]);
        }
    }
}
