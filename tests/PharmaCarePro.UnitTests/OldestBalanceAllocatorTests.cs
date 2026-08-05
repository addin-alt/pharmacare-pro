using PharmaCarePro.Application.Payments;

namespace PharmaCarePro.UnitTests;

public sealed class OldestBalanceAllocatorTests
{
    [Fact]
    public void Allocate_uses_oldest_document_first()
    {
        var olderId = Guid.NewGuid();
        var newerId = Guid.NewGuid();

        var allocations =
            OldestBalanceAllocator.Allocate(
                [
                    new OutstandingBalanceCandidate(
                        newerId,
                        new DateTime(2026, 8, 5),
                        60m),

                    new OutstandingBalanceCandidate(
                        olderId,
                        new DateTime(2026, 8, 1),
                        50m)
                ],
                80m);

        Assert.Equal(2, allocations.Count);

        Assert.Equal(
            new PaymentDocumentAllocation(
                olderId,
                50m),
            allocations[0]);

        Assert.Equal(
            new PaymentDocumentAllocation(
                newerId,
                30m),
            allocations[1]);
    }

    [Fact]
    public void Allocate_leaves_excess_for_account_balance()
    {
        var allocations =
            OldestBalanceAllocator.Allocate(
                [
                    new OutstandingBalanceCandidate(
                        Guid.NewGuid(),
                        new DateTime(2026, 8, 1),
                        20m)
                ],
                50m);

        var allocation = Assert.Single(allocations);

        Assert.Equal(20m, allocation.Amount);
        Assert.Equal(20m, allocations.Sum(item => item.Amount));
    }

    [Fact]
    public void Allocate_ignores_settled_documents()
    {
        var outstandingId = Guid.NewGuid();

        var allocations =
            OldestBalanceAllocator.Allocate(
                [
                    new OutstandingBalanceCandidate(
                        Guid.NewGuid(),
                        new DateTime(2026, 7, 1),
                        0m),

                    new OutstandingBalanceCandidate(
                        outstandingId,
                        new DateTime(2026, 8, 1),
                        15m)
                ],
                10m);

        var allocation = Assert.Single(allocations);

        Assert.Equal(outstandingId, allocation.DocumentId);
        Assert.Equal(10m, allocation.Amount);
    }

    [Fact]
    public void Allocate_rejects_duplicate_documents()
    {
        var documentId = Guid.NewGuid();

        var exception =
            Assert.Throws<ArgumentException>(
                () => OldestBalanceAllocator.Allocate(
                    [
                        new OutstandingBalanceCandidate(
                            documentId,
                            new DateTime(2026, 8, 1),
                            10m),

                        new OutstandingBalanceCandidate(
                            documentId,
                            new DateTime(2026, 8, 2),
                            5m)
                    ],
                    10m));

        Assert.Contains(
            "only once",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }
}
