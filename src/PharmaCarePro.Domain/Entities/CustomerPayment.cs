using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class CustomerPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string ReceiptNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal BalanceBefore { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal BalanceAfter { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal AppliedToSalesAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal AppliedToAccountBalanceAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } =
        PaymentMethod.Cash;

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    [StringLength(200)]
    public string RecordedBy { get; set; } = string.Empty;

    public DateTime ReceivedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public ICollection<CustomerPaymentAllocation> Allocations
        { get; set; } =
        new List<CustomerPaymentAllocation>();
}
