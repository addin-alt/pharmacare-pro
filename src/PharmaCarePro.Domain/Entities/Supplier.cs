using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class Supplier
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Supplier code is required.")]
    [StringLength(30)]
    [RegularExpression(
        "^[A-Za-z0-9][A-Za-z0-9-]*$",
        ErrorMessage =
            "Supplier code may contain letters, numbers and hyphens.")]
    public string SupplierCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Supplier name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ContactPerson { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(160)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(80)]
    public string? RegistrationNumber { get; set; }

    [StringLength(80)]
    public string? TaxIdentificationNumber { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal OpeningBalance { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal CurrentBalance { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
