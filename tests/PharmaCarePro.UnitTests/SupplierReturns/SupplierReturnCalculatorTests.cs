using PharmaCarePro.Application.SupplierReturns;

namespace PharmaCarePro.UnitTests.SupplierReturns;

public sealed class SupplierReturnCalculatorTests
{
    [Fact]
    public void Calculate_PartialPaidReturn_UsesProportionalValue()
    {
        var purchaseItemId = Guid.NewGuid();

        var result =
            SupplierReturnCalculator.Calculate(
            [
                new SupplierReturnLineCandidate(
                    purchaseItemId,
                    PurchasedQuantity: 10,
                    FreeQuantity: 2,
                    PreviouslyReturnedQuantity: 0,
                    PreviouslyReturnedFreeQuantity: 0,
                    OriginalLineEntitlement: 15,
                    PreviouslyReturnedAmount: 0,
                    RequestedQuantity: 2,
                    RequestedFreeQuantity: 0)
            ]);

        Assert.Equal(3m, result.GrossReturnAmount);
        Assert.Equal(2, result.Lines[0].Quantity);
        Assert.Equal(1.50m, result.Lines[0].UnitReturnAmount);
    }

    [Fact]
    public void Calculate_FinalPaidReturn_AbsorbsRoundingDifference()
    {
        var purchaseItemId = Guid.NewGuid();

        var result =
            SupplierReturnCalculator.Calculate(
            [
                new SupplierReturnLineCandidate(
                    purchaseItemId,
                    PurchasedQuantity: 3,
                    FreeQuantity: 0,
                    PreviouslyReturnedQuantity: 2,
                    PreviouslyReturnedFreeQuantity: 0,
                    OriginalLineEntitlement: 10,
                    PreviouslyReturnedAmount: 6.66m,
                    RequestedQuantity: 1,
                    RequestedFreeQuantity: 0)
            ]);

        Assert.Equal(3.34m, result.GrossReturnAmount);
        Assert.Equal(3.34m, result.Lines[0].LineReturnAmount);
    }

    [Fact]
    public void Calculate_FreeOnlyReturn_HasZeroFinancialValue()
    {
        var purchaseItemId = Guid.NewGuid();

        var result =
            SupplierReturnCalculator.Calculate(
            [
                new SupplierReturnLineCandidate(
                    purchaseItemId,
                    PurchasedQuantity: 10,
                    FreeQuantity: 2,
                    PreviouslyReturnedQuantity: 0,
                    PreviouslyReturnedFreeQuantity: 0,
                    OriginalLineEntitlement: 15,
                    PreviouslyReturnedAmount: 0,
                    RequestedQuantity: 0,
                    RequestedFreeQuantity: 1)
            ]);

        Assert.Equal(0m, result.GrossReturnAmount);
        Assert.Equal(1, result.Lines[0].FreeQuantity);
        Assert.Equal(0m, result.Lines[0].LineReturnAmount);
    }

    [Fact]
    public void Calculate_ExcessPaidQuantity_Throws()
    {
        var purchaseItemId = Guid.NewGuid();

        var exception =
            Assert.Throws<InvalidOperationException>(() =>
                SupplierReturnCalculator.Calculate(
                [
                    new SupplierReturnLineCandidate(
                        purchaseItemId,
                        PurchasedQuantity: 10,
                        FreeQuantity: 0,
                        PreviouslyReturnedQuantity: 9,
                        PreviouslyReturnedFreeQuantity: 0,
                        OriginalLineEntitlement: 15,
                        PreviouslyReturnedAmount: 13.50m,
                        RequestedQuantity: 2,
                        RequestedFreeQuantity: 0)
                ]));

        Assert.Contains(
            "cannot exceed",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Calculate_DuplicatePurchaseItem_Throws()
    {
        var purchaseItemId = Guid.NewGuid();

        var candidate =
            new SupplierReturnLineCandidate(
                purchaseItemId,
                PurchasedQuantity: 10,
                FreeQuantity: 0,
                PreviouslyReturnedQuantity: 0,
                PreviouslyReturnedFreeQuantity: 0,
                OriginalLineEntitlement: 15,
                PreviouslyReturnedAmount: 0,
                RequestedQuantity: 1,
                RequestedFreeQuantity: 0);

        Assert.Throws<ArgumentException>(() =>
            SupplierReturnCalculator.Calculate(
            [
                candidate,
                candidate
            ]));
    }
}
