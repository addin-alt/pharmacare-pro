using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class MedicineBatchValidationTests
{
    [Fact]
    public void Batch_with_expiry_before_manufacture_is_invalid()
    {
        var batch = CreateValidBatch();

        batch.ManufacturingDate = new DateTime(2026, 8, 1);
        batch.ExpiryDate = new DateTime(2026, 7, 31);

        var results = Validate(batch);

        Assert.Contains(
            results,
            result => result.ErrorMessage != null &&
                      result.ErrorMessage.Contains(
                          "Expiry date",
                          StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Available_quantity_cannot_exceed_received_stock()
    {
        var batch = CreateValidBatch();

        batch.ReceivedQuantity = 10;
        batch.FreeQuantity = 2;
        batch.AvailableQuantity = 13;

        var results = Validate(batch);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(MedicineBatch.AvailableQuantity)));
    }

    [Fact]
    public void Complete_batch_is_valid()
    {
        var batch = CreateValidBatch();

        var results = Validate(batch);

        Assert.Empty(results);
    }

    private static MedicineBatch CreateValidBatch()
    {
        return new MedicineBatch
        {
            MedicineId = Guid.NewGuid(),
            BatchNumber = "B240801",
            ManufacturingDate = new DateTime(2026, 8, 1),
            ExpiryDate = new DateTime(2028, 8, 1),
            ReceivedQuantity = 100,
            FreeQuantity = 10,
            AvailableQuantity = 110,
            PurchasePrice = 1.50m,
            SellingPrice = 2.00m,
        };
    }

    private static List<ValidationResult> Validate(
        MedicineBatch batch)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            batch,
            new ValidationContext(batch),
            results,
            validateAllProperties: true);

        return results;
    }
}
