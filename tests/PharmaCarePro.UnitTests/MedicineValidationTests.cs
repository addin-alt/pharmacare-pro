using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class MedicineValidationTests
{
    [Fact]
    public void Empty_medicine_is_invalid()
    {
        var medicine = new Medicine();
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            medicine,
            new ValidationContext(medicine),
            results,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void Complete_medicine_is_valid()
    {
        var medicine = new Medicine
        {
            BrandName = "Napa",
            GenericName = "Paracetamol",
            Strength = "500 mg",
            DosageForm = "Tablet",
            Sku = "MED-0001",
            PurchasePrice = 1.50m,
            SellingPrice = 2.00m,
            MaximumRetailPrice = 2.00m,
            ReorderLevel = 20,
        };

        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            medicine,
            new ValidationContext(medicine),
            results,
            validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(results);
    }
}
