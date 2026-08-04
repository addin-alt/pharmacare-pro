using System.ComponentModel.DataAnnotations;

namespace PharmaCarePro.Domain.Entities;

public sealed class PrescriptionItem : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PrescriptionId { get; set; }

    public Prescription? Prescription { get; set; }

    public Guid MedicineId { get; set; }

    public Medicine? Medicine { get; set; }

    [Required]
    [StringLength(180)]
    public string MedicineName { get; set; } = string.Empty;

    [Range(1, 1_000_000)]
    public int QuantityPrescribed { get; set; }

    [Range(0, 1_000_000)]
    public int QuantityDispensed { get; set; }

    [Required]
    [StringLength(300)]
    public string DosageInstructions { get; set; } = string.Empty;

    [Range(1, 3650)]
    public int? DurationDays { get; set; }

    public bool SubstitutionAllowed { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (MedicineId == Guid.Empty)
        {
            yield return new ValidationResult(
                "A medicine must be selected.",
                [nameof(MedicineId)]);
        }

        if (QuantityDispensed > QuantityPrescribed)
        {
            yield return new ValidationResult(
                "Dispensed quantity cannot exceed prescribed quantity.",
                [
                    nameof(QuantityDispensed),
                    nameof(QuantityPrescribed),
                ]);
        }
    }
}
