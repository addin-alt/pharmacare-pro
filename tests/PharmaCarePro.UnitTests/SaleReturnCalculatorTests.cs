using PharmaCarePro.Application.Returns;

namespace PharmaCarePro.UnitTests;

public sealed class SaleReturnCalculatorTests
{
    [Fact]
    public void Calculate_returns_selected_quantity_value()
    {
        var saleItemId = Guid.NewGuid();

        var result =
            SaleReturnCalculator.Calculate(
                [
                    new SaleReturnLineCandidate(
                        saleItemId,
                        5,
                        0,
                        10m,
                        0m,
                        2)
                ]);

        var line = Assert.Single(result.Lines);

        Assert.Equal(saleItemId, line.SaleItemId);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(2m, line.UnitRefundAmount);
        Assert.Equal(4m, line.LineRefundAmount);
        Assert.Equal(4m, result.GrossReturnAmount);
    }

    [Fact]
    public void Calculate_final_return_absorbs_rounding_remainder()
    {
        var saleItemId = Guid.NewGuid();

        var result =
            SaleReturnCalculator.Calculate(
                [
                    new SaleReturnLineCandidate(
                        saleItemId,
                        3,
                        1,
                        10m,
                        3.33m,
                        2)
                ]);

        var line = Assert.Single(result.Lines);

        Assert.Equal(6.67m, line.LineRefundAmount);
        Assert.Equal(6.67m, result.GrossReturnAmount);
    }

    [Fact]
    public void Calculate_rejects_quantity_above_remaining()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => SaleReturnCalculator.Calculate(
                    [
                        new SaleReturnLineCandidate(
                            Guid.NewGuid(),
                            5,
                            4,
                            10m,
                            8m,
                            2)
                    ]));

        Assert.Contains(
            "remaining",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Calculate_rejects_duplicate_sale_items()
    {
        var saleItemId = Guid.NewGuid();

        var exception =
            Assert.Throws<ArgumentException>(
                () => SaleReturnCalculator.Calculate(
                    [
                        new SaleReturnLineCandidate(
                            saleItemId,
                            5,
                            0,
                            10m,
                            0m,
                            1),

                        new SaleReturnLineCandidate(
                            saleItemId,
                            5,
                            0,
                            10m,
                            0m,
                            1)
                    ]));

        Assert.Contains(
            "only once",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }
}
