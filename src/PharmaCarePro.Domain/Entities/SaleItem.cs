using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class SaleItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SaleId { get; set; }

    public Sale? Sale { get; set; }

    public Guid MedicineId { get; set; }

    public Medicine? Medicine { get; set; }

    public Guid MedicineBatchId { get; set; }

    public MedicineBatch? MedicineBatch { get; set; }

    [Required]
    [StringLength(120)]
    public string BrandName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string GenericName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Strength { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string BatchNumber { get; set; } = string.Empty;

    [Range(1, 10_000_000)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal DiscountAmount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal LineTotal { get; set; }
}
