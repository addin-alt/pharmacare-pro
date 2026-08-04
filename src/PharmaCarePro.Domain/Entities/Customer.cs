using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Customer code is required.")]
    [StringLength(30)]
    [RegularExpression(
        "^[A-Za-z0-9][A-Za-z0-9-]*$",
        ErrorMessage =
            "Customer code may contain letters, numbers and hyphens.")]
    public string CustomerCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(160)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(500)]
    public string? Allergies { get; set; }

    [StringLength(500)]
    public string? MedicalNotes { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal CurrentBalance { get; set; }

    [Range(0, 100_000_000)]
    public int LoyaltyPoints { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Sale> Sales { get; set; } =
        new List<Sale>();
}
