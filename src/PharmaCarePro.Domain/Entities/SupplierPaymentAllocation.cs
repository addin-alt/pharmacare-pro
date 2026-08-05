using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class SupplierPaymentAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SupplierPaymentId { get; set; }

    public SupplierPayment SupplierPayment { get; set; } =
        null!;

    public Guid PurchaseId { get; set; }

    public Purchase Purchase { get; set; } = null!;

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;
}
