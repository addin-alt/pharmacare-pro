using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class PurchaseValidationTests
{
    [Fact]
    public void Purchase_requires_a_supplier()
    {
        var purchase = CreateValidPurchase();
        purchase.SupplierId = Guid.Empty;

        var results = Validate(purchase);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Purchase.SupplierId)));
    }

    [Fact]
    public void Purchase_rejects_incorrect_due_amount()
    {
        var purchase = CreateValidPurchase();

        purchase.GrandTotal = 1000;
        purchase.PaidAmount = 400;
        purchase.DueAmount = 100;

        var results = Validate(purchase);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Purchase.DueAmount)));
    }

    [Fact]
    public void Purchase_item_rejects_invalid_expiry_date()
    {
        var item = new PurchaseItem
        {
            PurchaseId = Guid.NewGuid(),
            MedicineId = Guid.NewGuid(),
            MedicineName = "Napa 500mg",
            BatchNumber = "NAPA-TEST-01",
            ManufacturingDate = new DateTime(2027, 1, 1),
            ExpiryDate = new DateTime(2026, 1, 1),
            Quantity = 10,
            PurchasePrice = 1.50m,
            SellingPrice = 2.00m,
            LineTotal = 15.00m
        };

        var results = Validate(item);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(PurchaseItem.ExpiryDate)));
    }

    private static Purchase CreateValidPurchase()
    {
        return new Purchase
        {
            PurchaseNumber = "PUR-0001",
            SupplierId = Guid.NewGuid(),
            Subtotal = 1000,
            DiscountAmount = 0,
            TaxAmount = 0,
            GrandTotal = 1000,
            PaidAmount = 400,
            DueAmount = 600,
            PaymentMethod = PaymentMethod.Cash,
            Status = PurchaseStatus.Received
        };
    }

    private static List<ValidationResult> Validate(object model)
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
