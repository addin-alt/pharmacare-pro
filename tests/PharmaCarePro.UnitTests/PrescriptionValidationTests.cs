using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class PrescriptionValidationTests
{
    [Fact]
    public void Prescription_requires_a_customer()
    {
        var prescription = CreateValidPrescription();
        prescription.CustomerId = Guid.Empty;

        var results = Validate(prescription);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Prescription.CustomerId)));
    }

    [Fact]
    public void Prescription_rejects_invalid_validity_period()
    {
        var prescription = CreateValidPrescription();

        prescription.IssuedDate =
            new DateTime(2026, 8, 4);

        prescription.ValidUntil =
            new DateTime(2026, 8, 3);

        var results = Validate(prescription);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Prescription.ValidUntil)));
    }

    [Fact]
    public void Prescription_item_rejects_over_dispensing()
    {
        var item = new PrescriptionItem
        {
            PrescriptionId = Guid.NewGuid(),
            MedicineId = Guid.NewGuid(),
            MedicineName = "Napa 500mg",
            QuantityPrescribed = 10,
            QuantityDispensed = 11,
            DosageInstructions = "One tablet twice daily",
            DurationDays = 5
        };

        var results = Validate(item);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(PrescriptionItem.QuantityDispensed)));
    }

    private static Prescription CreateValidPrescription()
    {
        return new Prescription
        {
            PrescriptionNumber = "RX-0001",
            CustomerId = Guid.NewGuid(),
            PatientName = "Rahim Ahmed",
            PrescriberName = "Dr. Test Doctor",
            IssuedDate = new DateTime(2026, 8, 4),
            ValidUntil = new DateTime(2026, 9, 4),
            Status = PrescriptionStatus.Active
        };
    }

    private static List<ValidationResult> Validate(
        object model)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        return results;
    }
}
