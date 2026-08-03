using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class Medicine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Brand name is required.")]
    [StringLength(120)]
    public string BrandName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Generic name is required.")]
    [StringLength(120)]
    public string GenericName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Strength is required.")]
    [StringLength(50)]
    public string Strength { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dosage form is required.")]
    [StringLength(50)]
    public string DosageForm { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Category { get; set; }

    [StringLength(120)]
    public string? Manufacturer { get; set; }

    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(64)]
    public string Sku { get; set; } = string.Empty;

    [StringLength(64)]
    public string? Barcode { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal SellingPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal MaximumRetailPrice { get; set; }

    [Range(0, 1_000_000)]
    public int ReorderLevel { get; set; }

    public bool RequiresPrescription { get; set; }

    [StringLength(50)]
    public string? RackLocation { get; set; }

    [StringLength(250)]
    public string? StorageInstructions { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
