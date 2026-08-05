using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests.SupplierReturns;

public sealed class SupplierReturnValidationTests
{
    [Fact]
    public void SupplierReturn_WithBalancedSettlement_IsValid()
    {
        var model = CreateReturn(
            gross: 10,
            payableReduction: 6,
            supplierRefund: 4,
            refundMethod: PaymentMethod.Cash);

        var results = Validate(model);

        Assert.Empty(results);
    }

    [Fact]
    public void SupplierReturn_WithUnbalancedSettlement_IsInvalid()
    {
        var model = CreateReturn(
            gross: 10,
            payableReduction: 5,
            supplierRefund: 4,
            refundMethod: PaymentMethod.Cash);

        var results = Validate(model);

        Assert.Contains(
            results,
            result => result.ErrorMessage?.Contains(
                "must equal the return total",
                StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void SupplierReturn_WithRefundAndNoMethod_IsInvalid()
    {
        var model = CreateReturn(
            gross: 4,
            payableReduction: 0,
            supplierRefund: 4,
            refundMethod: null);

        var results = Validate(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(SupplierReturn.RefundMethod)));
    }

    [Fact]
    public void SupplierReturnItem_WithNoQuantity_IsInvalid()
    {
        var model = CreateItem(
            quantity: 0,
            freeQuantity: 0,
            unitAmount: 1.50m,
            lineAmount: 0);

        var results = Validate(model);

        Assert.Contains(
            results,
            result => result.ErrorMessage?.Contains(
                "At least one paid or free unit",
                StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void SupplierReturnItem_WithCorrectLineAmount_IsValid()
    {
        var model = CreateItem(
            quantity: 2,
            freeQuantity: 1,
            unitAmount: 1.50m,
            lineAmount: 3.00m);

        var results = Validate(model);

        Assert.Empty(results);
    }

    private static SupplierReturn CreateReturn(
        decimal gross,
        decimal payableReduction,
        decimal supplierRefund,
        PaymentMethod? refundMethod)
    {
        return new SupplierReturn
        {
            ReturnNumber = "PRT-TEST-001",
            PurchaseId = Guid.NewGuid(),
            SupplierId = Guid.NewGuid(),
            GrossReturnAmount = gross,
            PayableReductionAmount = payableReduction,
            SupplierRefundAmount = supplierRefund,
            RefundMethod = refundMethod,
            Reason = "Test supplier return",
            RecordedBy = "test-user"
        };
    }

    private static SupplierReturnItem CreateItem(
        int quantity,
        int freeQuantity,
        decimal unitAmount,
        decimal lineAmount)
    {
        return new SupplierReturnItem
        {
            SupplierReturnId = Guid.NewGuid(),
            PurchaseItemId = Guid.NewGuid(),
            MedicineBatchId = Guid.NewGuid(),
            Quantity = quantity,
            FreeQuantity = freeQuantity,
            UnitReturnAmount = unitAmount,
            LineReturnAmount = lineAmount
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
