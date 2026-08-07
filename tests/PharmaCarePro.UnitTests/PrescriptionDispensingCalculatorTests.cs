using PharmaCarePro.Application.Prescriptions;

namespace PharmaCarePro.UnitTests;

public sealed class PrescriptionDispensingCalculatorTests
{
    [Fact]
    public void Calculate_UpdatesSelectedAndKeepsUnselectedLines()
    {
        var firstItemId = Guid.NewGuid();
        var secondItemId = Guid.NewGuid();

        var plan =
            PrescriptionDispensingCalculator.Calculate(
            [
                new PrescriptionDispensingLine(
                    firstItemId,
                    Guid.NewGuid(),
                    10,
                    2,
                    3),
                new PrescriptionDispensingLine(
                    secondItemId,
                    Guid.NewGuid(),
                    5,
                    1,
                    0)
            ]);

        Assert.False(plan.IsFullyDispensed);

        Assert.Equal(
            5,
            plan.NewDispensedQuantities[firstItemId]);

        Assert.Equal(
            1,
            plan.NewDispensedQuantities[secondItemId]);
    }

    [Fact]
    public void Calculate_WhenAllRemainingRequested_IsFullyDispensed()
    {
        var firstItemId = Guid.NewGuid();
        var secondItemId = Guid.NewGuid();

        var plan =
            PrescriptionDispensingCalculator.Calculate(
            [
                new PrescriptionDispensingLine(
                    firstItemId,
                    Guid.NewGuid(),
                    10,
                    4,
                    6),
                new PrescriptionDispensingLine(
                    secondItemId,
                    Guid.NewGuid(),
                    3,
                    0,
                    3)
            ]);

        Assert.True(plan.IsFullyDispensed);

        Assert.Equal(
            10,
            plan.NewDispensedQuantities[firstItemId]);

        Assert.Equal(
            3,
            plan.NewDispensedQuantities[secondItemId]);
    }

    [Fact]
    public void Calculate_WhenNothingRequested_Throws()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        10,
                        2,
                        0)
                ]));

        Assert.Contains(
            "selected for dispensing",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenRequestExceedsRemaining_Throws()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        10,
                        8,
                        3)
                ]));

        Assert.Contains(
            "exceeds the remaining prescribed quantity",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenDuplicateItemIds_Throws()
    {
        var itemId = Guid.NewGuid();

        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        itemId,
                        Guid.NewGuid(),
                        5,
                        0,
                        1),
                    new PrescriptionDispensingLine(
                        itemId,
                        Guid.NewGuid(),
                        5,
                        0,
                        1)
                ]));

        Assert.Contains(
            "only once",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenLinesAreEmpty_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                    Array.Empty<
                        PrescriptionDispensingLine>()));

        Assert.Contains(
            "At least one prescription item",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenLinesAreNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PrescriptionDispensingCalculator.Calculate(
                null!));
    }

    [Fact]
    public void Calculate_WhenPrescriptionItemIdIsEmpty_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.Empty,
                        Guid.NewGuid(),
                        5,
                        0,
                        1)
                ]));

        Assert.Contains(
            "prescription-item identifier",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenMedicineIdIsEmpty_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.NewGuid(),
                        Guid.Empty,
                        5,
                        0,
                        1)
                ]));

        Assert.Contains(
            "medicine identifier",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenPrescribedQuantityIsZero_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        0,
                        0,
                        1)
                ]));

        Assert.Contains(
            "greater than zero",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenDispensedQuantityIsNegative_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        5,
                        -1,
                        1)
                ]));

        Assert.Contains(
            "cannot be negative",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenDispensedExceedsPrescribed_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        5,
                        6,
                        1)
                ]));

        Assert.Contains(
            "cannot exceed",
            exception.Message);
    }

    [Fact]
    public void Calculate_WhenRequestedQuantityIsNegative_Throws()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                PrescriptionDispensingCalculator.Calculate(
                [
                    new PrescriptionDispensingLine(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        5,
                        0,
                        -1)
                ]));

        Assert.Contains(
            "Requested dispensing quantity cannot be negative",
            exception.Message);
    }
}
