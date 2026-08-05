namespace PharmaCarePro.Application.Returns;

public sealed record SaleReturnLineCandidate(
    Guid SaleItemId,
    int SoldQuantity,
    int PreviouslyReturnedQuantity,
    decimal OriginalLineEntitlement,
    decimal PreviouslyReturnedAmount,
    int RequestedQuantity);

public sealed record SaleReturnLineCalculation(
    Guid SaleItemId,
    int Quantity,
    decimal UnitRefundAmount,
    decimal LineRefundAmount);

public sealed record SaleReturnCalculation(
    decimal GrossReturnAmount,
    IReadOnlyList<SaleReturnLineCalculation> Lines);

public static class SaleReturnCalculator
{
    public static SaleReturnCalculation Calculate(
        IEnumerable<SaleReturnLineCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var preparedCandidates = candidates.ToList();

        if (preparedCandidates.Count == 0)
        {
            throw new ArgumentException(
                "At least one return item is required.",
                nameof(candidates));
        }

        var duplicateItem =
            preparedCandidates
                .GroupBy(candidate => candidate.SaleItemId)
                .FirstOrDefault(group => group.Count() > 1);

        if (duplicateItem is not null)
        {
            throw new ArgumentException(
                "Each sold item may appear only once in a return.",
                nameof(candidates));
        }

        var calculations =
            new List<SaleReturnLineCalculation>();

        foreach (var candidate in preparedCandidates)
        {
            ValidateCandidate(candidate);

            var remainingQuantity =
                candidate.SoldQuantity -
                candidate.PreviouslyReturnedQuantity;

            var remainingAmount =
                RoundMoney(
                    candidate.OriginalLineEntitlement -
                    candidate.PreviouslyReturnedAmount);

            if (candidate.RequestedQuantity >
                remainingQuantity)
            {
                throw new InvalidOperationException(
                    "Return quantity cannot exceed the quantity " +
                    "remaining from the original sale.");
            }

            if (remainingAmount < 0)
            {
                throw new InvalidOperationException(
                    "Previously returned value exceeds the sold-item " +
                    "refund entitlement.");
            }

            decimal lineRefundAmount;

            if (candidate.RequestedQuantity ==
                remainingQuantity)
            {
                /*
                 * The final return receives the exact remaining value,
                 * absorbing any earlier one-cent rounding difference.
                 */
                lineRefundAmount = remainingAmount;
            }
            else
            {
                var originalUnitEntitlement =
                    candidate.OriginalLineEntitlement /
                    candidate.SoldQuantity;

                lineRefundAmount =
                    RoundMoney(
                        originalUnitEntitlement *
                        candidate.RequestedQuantity);

                lineRefundAmount =
                    Math.Min(
                        lineRefundAmount,
                        remainingAmount);
            }

            var unitRefundAmount =
                candidate.RequestedQuantity == 0
                    ? 0
                    : RoundMoney(
                        lineRefundAmount /
                        candidate.RequestedQuantity);

            calculations.Add(
                new SaleReturnLineCalculation(
                    candidate.SaleItemId,
                    candidate.RequestedQuantity,
                    unitRefundAmount,
                    lineRefundAmount));
        }

        var grossReturnAmount =
            RoundMoney(
                calculations.Sum(line =>
                    line.LineRefundAmount));

        if (grossReturnAmount <= 0)
        {
            throw new InvalidOperationException(
                "The selected items have no refundable value.");
        }

        return new SaleReturnCalculation(
            grossReturnAmount,
            calculations);
    }

    private static void ValidateCandidate(
        SaleReturnLineCandidate candidate)
    {
        if (candidate.SaleItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid sold-item identifier is required.");
        }

        if (candidate.SoldQuantity <= 0)
        {
            throw new ArgumentException(
                "Sold quantity must be greater than zero.");
        }

        if (candidate.PreviouslyReturnedQuantity < 0)
        {
            throw new ArgumentException(
                "Previously returned quantity cannot be negative.");
        }

        if (candidate.PreviouslyReturnedQuantity >
            candidate.SoldQuantity)
        {
            throw new ArgumentException(
                "Previously returned quantity cannot exceed sold " +
                "quantity.");
        }

        if (candidate.RequestedQuantity <= 0)
        {
            throw new ArgumentException(
                "Return quantity must be greater than zero.");
        }

        if (candidate.OriginalLineEntitlement < 0 ||
            candidate.PreviouslyReturnedAmount < 0)
        {
            throw new ArgumentException(
                "Return values cannot be negative.");
        }
    }

    private static decimal RoundMoney(decimal amount)
    {
        return Math.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero);
    }
}
