namespace PharmaCarePro.Application.Inventory;

public sealed record FefoBatchCandidate(
    Guid BatchId,
    string BatchNumber,
    DateTime ExpiryDate,
    int AvailableQuantity,
    decimal UnitPrice,
    bool IsQuarantined);

public sealed record FefoAllocation(
    Guid BatchId,
    string BatchNumber,
    DateTime ExpiryDate,
    int Quantity,
    decimal UnitPrice)
{
    public decimal LineTotal =>
        Math.Round(Quantity * UnitPrice, 2);
}

public static class FefoAllocator
{
    public static IReadOnlyList<FefoAllocation> Allocate(
        IEnumerable<FefoBatchCandidate> candidates,
        int requestedQuantity,
        DateTime inventoryDate)
    {
        if (requestedQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedQuantity),
                "Requested quantity must be greater than zero.");
        }

        var eligibleBatches = candidates
            .Where(batch =>
                batch.AvailableQuantity > 0 &&
                !batch.IsQuarantined &&
                batch.ExpiryDate.Date >= inventoryDate.Date)
            .OrderBy(batch => batch.ExpiryDate)
            .ThenBy(batch => batch.BatchNumber)
            .ToList();

        var remainingQuantity = requestedQuantity;
        var allocations = new List<FefoAllocation>();

        foreach (var batch in eligibleBatches)
        {
            if (remainingQuantity == 0)
            {
                break;
            }

            var allocatedQuantity = Math.Min(
                remainingQuantity,
                batch.AvailableQuantity);

            allocations.Add(
                new FefoAllocation(
                    batch.BatchId,
                    batch.BatchNumber,
                    batch.ExpiryDate,
                    allocatedQuantity,
                    batch.UnitPrice));

            remainingQuantity -= allocatedQuantity;
        }

        if (remainingQuantity > 0)
        {
            throw new InvalidOperationException(
                "Insufficient non-expired inventory is available.");
        }

        return allocations;
    }
}
