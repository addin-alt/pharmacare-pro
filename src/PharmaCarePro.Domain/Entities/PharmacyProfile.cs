using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class PharmacyProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Pharmacy name is required.")]
    [StringLength(150)]
    public string PharmacyName { get; set; } =
        "PharmaCare Pro";

    [Required(ErrorMessage = "Branch name is required.")]
    [StringLength(150)]
    public string BranchName { get; set; } =
        "Main Pharmacy";

    [StringLength(80)]
    public string? LicenseNumber { get; set; }

    [StringLength(80)]
    public string? TaxIdentificationNumber { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(160)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [Required]
    [RegularExpression(
        "^[A-Za-z]{3}$",
        ErrorMessage =
            "Currency code must contain three letters.")]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "BDT";

    [Required]
    [StringLength(8)]
    public string CurrencySymbol { get; set; } = "৳";

    [Required]
    [StringLength(100)]
    public string TimeZoneId { get; set; } =
        "Asia/Dhaka";

    [Required]
    [RegularExpression(
        "^[A-Za-z0-9-]{2,12}$",
        ErrorMessage =
            "Invoice prefix may contain letters, numbers and hyphens.")]
    [StringLength(12)]
    public string InvoicePrefix { get; set; } = "PCP";

    [Required]
    [RegularExpression(
        "^[A-Za-z0-9-]{2,12}$",
        ErrorMessage =
            "Purchase prefix may contain letters, numbers and hyphens.")]
    [StringLength(12)]
    public string PurchasePrefix { get; set; } = "PUR";

    [Required]
    [RegularExpression(
        "^[A-Za-z0-9-]{2,12}$",
        ErrorMessage =
            "Prescription prefix may contain letters, numbers and hyphens.")]
    [StringLength(12)]
    public string PrescriptionPrefix { get; set; } = "RX";

    [Range(
        1,
        3650,
        ErrorMessage =
            "Expiry alert days must be between 1 and 3650.")]
    public int ExpiryAlertDays { get; set; } = 90;

    public bool LowStockAlertsEnabled { get; set; } = true;

    public bool ExpiryAlertsEnabled { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
