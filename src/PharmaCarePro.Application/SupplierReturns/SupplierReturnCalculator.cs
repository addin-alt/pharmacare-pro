namespace PharmaCarePro.Application.SupplierReturns;

public sealed record SupplierReturnLineCandidate(
    Guid PurchaseItemId,
    int PurchasedQuantity,
    int FreeQuantity,
    int PreviouslyReturnedQuantity,
    int PreviouslyReturnedFreeQuantity,
    decimal OriginalLineEntitlement,
    decimal PreviouslyReturnedAmount,
    int RequestedQuantity,
    int RequestedFreeQuantity);

public sealed record SupplierReturnLineCalculation(
    Guid PurchaseItemId,
    int Quantity,
    int FreeQuantity,
    decimal UnitReturnAmount,
    decimal LineReturnAmount);

public sealed record SupplierReturnCalculation(
    decimal GrossReturnAmount,
    IReadOnlyList<SupplierReturnLineCalculation> Lines);

public static class SupplierReturnCalculator
{
    public static SupplierReturnCalculation Calculate(
        IEnumerable<SupplierReturnLineCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        var preparedCandidates = candidates.ToList();

        if (preparedCandidates.Count == 0)
        {
            throw new ArgumentException(
                "At least one supplier-return item is required.",
                nameof(candidates));
        }

        var duplicateItem =
            preparedCandidates
                .GroupBy(candidate =>
                    candidate.PurchaseItemId)
                .FirstOrDefault(group =>
                    group.Count() > 1);

        if (duplicateItem is not null)
        {
            throw new ArgumentException(
                "Each purchased item may appear only once in a " +
                "supplier return.",
                nameof(candidates));
        }

        var calculations =
            new List<SupplierReturnLineCalculation>();

        foreach (var candidate in preparedCandidates)
        {
            ValidateCandidate(candidate);

            var remainingQuantity =
                candidate.PurchasedQuantity -
                candidate.PreviouslyReturnedQuantity;

            var remainingFreeQuantity =
                candidate.FreeQuantity -
                candidate.PreviouslyReturnedFreeQuantity;

            if (candidate.RequestedQuantity >
                remainingQuantity)
            {
                throw new InvalidOperationException(
                    "Paid return quantity cannot exceed the quantity " +
                    "remaining from the original purchase.");
            }

            if (candidate.RequestedFreeQuantity >
                remainingFreeQuantity)
            {
                throw new InvalidOperationException(
                    "Free return quantity cannot exceed the free " +
                    "quantity remaining from the original purchase.");
            }

            var remainingAmount =
                RoundMoney(
                    candidate.OriginalLineEntitlement -
                    candidate.PreviouslyReturnedAmount);

            if (remainingAmount < 0)
            {
                throw new InvalidOperationException(
                    "Previously returned value exceeds the purchased " +
                    "item's return entitlement.");
            }

            decimal lineReturnAmount;

            if (candidate.RequestedQuantity == 0)
            {
                lineReturnAmount = 0;
            }
            else if (candidate.RequestedQuantity ==
                remainingQuantity)
            {
                /*
                 * The final paid-quantity return receives the exact
                 * remaining value, absorbing earlier rounding.
                 */
                lineReturnAmount = remainingAmount;
            }
            else
            {
                var originalUnitEntitlement =
                    candidate.OriginalLineEntitlement /
                    candidate.PurchasedQuantity;

                lineReturnAmount =
                    RoundMoney(
                        originalUnitEntitlement *
                        candidate.RequestedQuantity);

                lineReturnAmount =
                    Math.Min(
                        lineReturnAmount,
                        remainingAmount);
            }

            var unitReturnAmount =
                candidate.RequestedQuantity == 0
                    ? 0
                    : RoundMoney(
                        lineReturnAmount /
                        candidate.RequestedQuantity);

            calculations.Add(
                new SupplierReturnLineCalculation(
                    candidate.PurchaseItemId,
                    candidate.RequestedQuantity,
                    candidate.RequestedFreeQuantity,
                    unitReturnAmount,
                    lineReturnAmount));
        }

        return new SupplierReturnCalculation(
            RoundMoney(
                calculations.Sum(line =>
                    line.LineReturnAmount)),
            calculations);
    }

    private static void ValidateCandidate(
        SupplierReturnLineCandidate candidate)
    {
        if (candidate.PurchaseItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid purchased-item identifier is required.");
        }

        if (candidate.PurchasedQuantity <= 0)
        {
            throw new ArgumentException(
                "Purchased quantity must be greater than zero.");
        }

        if (candidate.FreeQuantity < 0)
        {
            throw new ArgumentException(
                "Free quantity cannot be negative.");
        }

        if (candidate.PreviouslyReturnedQuantity < 0 ||
            candidate.PreviouslyReturnedFreeQuantity < 0)
        {
            throw new ArgumentException(
                "Previously returned quantities cannot be negative.");
        }

        if (candidate.PreviouslyReturnedQuantity >
            candidate.PurchasedQuantity)
        {
            throw new ArgumentException(
                "Previously returned paid quantity cannot exceed " +
                "purchased quantity.");
        }

        if (candidate.PreviouslyReturnedFreeQuantity >
            candidate.FreeQuantity)
        {
            throw new ArgumentException(
                "Previously returned free quantity cannot exceed " +
                "the original free quantity.");
        }

        if (candidate.RequestedQuantity < 0 ||
            candidate.RequestedFreeQuantity < 0)
        {
            throw new ArgumentException(
                "Requested return quantities cannot be negative.");
        }

        if (candidate.RequestedQuantity +
            candidate.RequestedFreeQuantity <= 0)
        {
            throw new ArgumentException(
                "At least one paid or free unit must be returned.");
        }

        if (candidate.OriginalLineEntitlement < 0 ||
            candidate.PreviouslyReturnedAmount < 0)
        {
            throw new ArgumentException(
                "Supplier-return values cannot be negative.");
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
