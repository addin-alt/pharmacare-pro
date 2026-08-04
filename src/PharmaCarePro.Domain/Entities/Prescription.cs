using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class Prescription : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string PrescriptionNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }

    public Customer? Customer { get; set; }

    [Required]
    [StringLength(150)]
    public string PatientName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? PatientPhone { get; set; }

    [Required]
    [StringLength(150)]
    public string PrescriberName { get; set; } = string.Empty;

    [StringLength(80)]
    public string? PrescriberRegistrationNumber { get; set; }

    [StringLength(30)]
    public string? PrescriberPhone { get; set; }

    [StringLength(150)]
    public string? HospitalOrClinic { get; set; }

    public DateTime IssuedDate { get; set; } = DateTime.UtcNow.Date;

    public DateTime? ValidUntil { get; set; }

    public PrescriptionStatus Status { get; set; } =
        PrescriptionStatus.Active;

    [StringLength(1000)]
    public string? ClinicalNotes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PrescriptionItem> Items { get; set; } =
        new List<PrescriptionItem>();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (CustomerId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A customer must be selected.",
                [nameof(CustomerId)]);
        }

        if (ValidUntil.HasValue &&
            ValidUntil.Value.Date < IssuedDate.Date)
        {
            yield return new ValidationResult(
                "Valid-until date cannot be before the issued date.",
                [nameof(ValidUntil), nameof(IssuedDate)]);
        }
    }
}
