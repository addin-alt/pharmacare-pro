namespace PharmaCarePro.Application.Payments;

public sealed record OutstandingBalanceCandidate(
    Guid DocumentId,
    DateTime DocumentDateUtc,
    decimal OutstandingAmount);

public sealed record PaymentDocumentAllocation(
    Guid DocumentId,
    decimal Amount);

public static class OldestBalanceAllocator
{
    public static IReadOnlyList<PaymentDocumentAllocation> Allocate(
        IEnumerable<OutstandingBalanceCandidate> candidates,
        decimal paymentAmount)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        paymentAmount = RoundMoney(paymentAmount);

        if (paymentAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paymentAmount),
                "Payment amount must be greater than zero.");
        }

        var preparedCandidates =
            candidates
                .Select(candidate =>
                    new OutstandingBalanceCandidate(
                        candidate.DocumentId,
                        candidate.DocumentDateUtc,
                        RoundMoney(candidate.OutstandingAmount)))
                .ToList();

        if (preparedCandidates.Any(candidate =>
            candidate.DocumentId == Guid.Empty))
        {
            throw new ArgumentException(
                "Payment documents must have valid identifiers.",
                nameof(candidates));
        }

        if (preparedCandidates.Any(candidate =>
            candidate.OutstandingAmount < 0))
        {
            throw new ArgumentException(
                "Outstanding balances cannot be negative.",
                nameof(candidates));
        }

        var duplicateDocument =
            preparedCandidates
                .GroupBy(candidate => candidate.DocumentId)
                .FirstOrDefault(group => group.Count() > 1);

        if (duplicateDocument is not null)
        {
            throw new ArgumentException(
                "Each payment document may appear only once.",
                nameof(candidates));
        }

        var orderedCandidates =
            preparedCandidates
                .Where(candidate =>
                    candidate.OutstandingAmount > 0)
                .OrderBy(candidate =>
                    candidate.DocumentDateUtc)
                .ThenBy(candidate =>
                    candidate.DocumentId)
                .ToList();

        var allocations =
            new List<PaymentDocumentAllocation>();

        var remainingAmount = paymentAmount;

        foreach (var candidate in orderedCandidates)
        {
            if (remainingAmount <= 0)
            {
                break;
            }

            var allocatedAmount =
                Math.Min(
                    candidate.OutstandingAmount,
                    remainingAmount);

            allocatedAmount = RoundMoney(allocatedAmount);

            allocations.Add(
                new PaymentDocumentAllocation(
                    candidate.DocumentId,
                    allocatedAmount));

            remainingAmount =
                RoundMoney(
                    remainingAmount - allocatedAmount);
        }

        return allocations;
    }

    private static decimal RoundMoney(decimal amount)
    {
        return Math.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero);
    }
}
