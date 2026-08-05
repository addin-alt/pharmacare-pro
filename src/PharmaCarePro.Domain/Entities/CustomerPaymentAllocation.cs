using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class CustomerPaymentAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerPaymentId { get; set; }

    public CustomerPayment CustomerPayment { get; set; } =
        null!;

    public Guid SaleId { get; set; }

    public Sale Sale { get; set; } = null!;

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;
}
