using PharmaCarePro.Application.Inventory;

namespace PharmaCarePro.UnitTests;

public sealed class FefoAllocatorTests
{
    [Fact]
    public void Allocate_uses_earliest_expiry_first()
    {
        var earlierId = Guid.NewGuid();
        var laterId = Guid.NewGuid();

        var allocations = FefoAllocator.Allocate(
            [
                new FefoBatchCandidate(
                    laterId,
                    "LATER",
                    new DateTime(2028, 1, 1),
                    100,
                    2.00m,
                    false),

                new FefoBatchCandidate(
                    earlierId,
                    "EARLIER",
                    new DateTime(2027, 1, 1),
                    5,
                    1.80m,
                    false),
            ],
            8,
            new DateTime(2026, 8, 4));

        Assert.Equal(2, allocations.Count);

        Assert.Equal(earlierId, allocations[0].BatchId);
        Assert.Equal(5, allocations[0].Quantity);

        Assert.Equal(laterId, allocations[1].BatchId);
        Assert.Equal(3, allocations[1].Quantity);
    }

    [Fact]
    public void Allocate_skips_expired_and_quarantined_batches()
    {
        var validId = Guid.NewGuid();

        var allocations = FefoAllocator.Allocate(
            [
                new FefoBatchCandidate(
                    Guid.NewGuid(),
                    "EXPIRED",
                    new DateTime(2026, 7, 1),
                    100,
                    1.00m,
                    false),

                new FefoBatchCandidate(
                    Guid.NewGuid(),
                    "QUARANTINED",
                    new DateTime(2028, 1, 1),
                    100,
                    1.00m,
                    true),

                new FefoBatchCandidate(
                    validId,
                    "VALID",
                    new DateTime(2027, 1, 1),
                    10,
                    2.00m,
                    false),
            ],
            4,
            new DateTime(2026, 8, 4));

        var allocation = Assert.Single(allocations);

        Assert.Equal(validId, allocation.BatchId);
        Assert.Equal(4, allocation.Quantity);
    }

    [Fact]
    public void Allocate_rejects_insufficient_inventory()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => FefoAllocator.Allocate(
                [
                    new FefoBatchCandidate(
                        Guid.NewGuid(),
                        "BATCH-1",
                        new DateTime(2028, 1, 1),
                        2,
                        2.00m,
                        false),
                ],
                3,
                new DateTime(2026, 8, 4)));

        Assert.Contains(
            "Insufficient",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }
}
