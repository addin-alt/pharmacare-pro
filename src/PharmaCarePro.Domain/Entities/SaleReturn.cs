using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class SaleReturn : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string ReturnNumber { get; set; } = string.Empty;

    public Guid SaleId { get; set; }

    public Sale Sale { get; set; } = null!;

    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal GrossReturnAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal DueReductionAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal RefundedAmount { get; set; }

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

    public ICollection<SaleReturnItem> Items { get; set; } =
        new List<SaleReturnItem>();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SaleId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A sale is required.",
                [nameof(SaleId)]);
        }

        var settlementTotal =
            Math.Round(
                DueReductionAmount + RefundedAmount,
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
                "The due reduction and refunded amount must equal " +
                "the return total.",
                [
                    nameof(DueReductionAmount),
                    nameof(RefundedAmount)
                ]);
        }

        if (RefundedAmount > 0 &&
            RefundMethod is null)
        {
            yield return new ValidationResult(
                "A refund method is required when money is refunded.",
                [nameof(RefundMethod)]);
        }

        if (RefundMethod == PaymentMethod.Due)
        {
            yield return new ValidationResult(
                "Due is not a valid refund method.",
                [nameof(RefundMethod)]);
        }

        if (RefundedAmount == 0 &&
            RefundMethod is not null)
        {
            yield return new ValidationResult(
                "A refund method must not be selected when no money " +
                "is refunded.",
                [nameof(RefundMethod)]);
        }
    }
}
