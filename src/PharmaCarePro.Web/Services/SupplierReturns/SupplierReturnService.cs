using System.Data;
using Microsoft.EntityFrameworkCore;
using PharmaCarePro.Application.Documents;
using PharmaCarePro.Application.SupplierReturns;
using PharmaCarePro.Domain.Entities;
using PharmaCarePro.Web.Data;

namespace PharmaCarePro.Web.Services.SupplierReturns;

public sealed class SupplierReturnService(
    IServiceScopeFactory scopeFactory)
{
    public async Task<SupplierReturnResult> RecordAsync(
        SupplierReturnRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        using var scope = scopeFactory.CreateScope();

        var database =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        await using var transaction =
            await database.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var purchase =
            await database.Purchases
                .Include(item => item.Supplier)
                .Include(item => item.Items)
                    .ThenInclude(item => item.ReturnItems)
                .Include(item => item.Returns)
                    .ThenInclude(item => item.Items)
                .SingleOrDefaultAsync(
                    item => item.Id == request.PurchaseId,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Purchase was not found.");

        if (purchase.Status == PurchaseStatus.Cancelled ||
            purchase.Status == PurchaseStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only received purchases can be returned to a supplier.");
        }

        if (purchase.Status == PurchaseStatus.Returned)
        {
            throw new InvalidOperationException(
                "This purchase has already been fully returned.");
        }

        var supplier =
            purchase.Supplier
            ?? throw new InvalidOperationException(
                "The supplier linked to this purchase was not found.");

        var requestedItems =
            request.Items.ToDictionary(
                item => item.PurchaseItemId);

        var purchaseItems =
            purchase.Items
                .Where(item =>
                    requestedItems.ContainsKey(item.Id))
                .ToList();

        if (purchaseItems.Count != requestedItems.Count)
        {
            throw new InvalidOperationException(
                "One or more selected items do not belong to this " +
                "purchase.");
        }

        var originalPurchaseGross =
            RoundMoney(
                purchase.GrandTotal +
                purchase.Returns.Sum(item =>
                    item.GrossReturnAmount));

        var lineEntitlements =
            CalculateOriginalLineEntitlements(
                purchase.Items,
                originalPurchaseGross);

        var previousQuantityByItem =
            purchase.Returns
                .SelectMany(item => item.Items)
                .GroupBy(item => item.PurchaseItemId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(item =>
                        item.Quantity));

        var previousFreeQuantityByItem =
            purchase.Returns
                .SelectMany(item => item.Items)
                .GroupBy(item => item.PurchaseItemId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(item =>
                        item.FreeQuantity));

        var previousAmountByItem =
            purchase.Returns
                .SelectMany(item => item.Items)
                .GroupBy(item => item.PurchaseItemId)
                .ToDictionary(
                    group => group.Key,
                    group => RoundMoney(
                        group.Sum(item =>
                            item.LineReturnAmount)));

        var calculation =
            SupplierReturnCalculator.Calculate(
                purchaseItems.Select(item =>
                    new SupplierReturnLineCandidate(
                        item.Id,
                        item.Quantity,
                        item.FreeQuantity,
                        previousQuantityByItem.GetValueOrDefault(
                            item.Id),
                        previousFreeQuantityByItem.GetValueOrDefault(
                            item.Id),
                        lineEntitlements[item.Id],
                        previousAmountByItem.GetValueOrDefault(
                            item.Id),
                        requestedItems[item.Id].Quantity,
                        requestedItems[item.Id].FreeQuantity)));

        if (calculation.GrossReturnAmount >
            purchase.GrandTotal)
        {
            throw new InvalidOperationException(
                "Supplier return value cannot exceed the remaining " +
                "purchase value.");
        }

        var payableReductionAmount =
            RoundMoney(
                Math.Min(
                    purchase.DueAmount,
                    calculation.GrossReturnAmount));

        var supplierRefundAmount =
            RoundMoney(
                calculation.GrossReturnAmount -
                payableReductionAmount);

        ValidateRefundMethod(
            request.RefundMethod,
            supplierRefundAmount);

        if (supplierRefundAmount >
            purchase.PaidAmount)
        {
            throw new InvalidOperationException(
                "The supplier refund exceeds the amount paid against " +
                "this purchase.");
        }

        if (payableReductionAmount >
            supplier.CurrentBalance)
        {
            throw new InvalidOperationException(
                "The payable reduction exceeds the supplier account " +
                "balance.");
        }

        var normalizedReason =
            NormalizeRequired(
                request.Reason,
                200,
                "Return reason");

        var normalizedRecordedBy =
            NormalizeRequired(
                request.RecordedBy,
                200,
                "Recorded by");

        var normalizedNotes =
            NormalizeOptional(
                request.Notes,
                500,
                "Notes");

        var medicineIds =
            purchaseItems
                .Select(item => item.MedicineId)
                .Distinct()
                .ToList();

        var batches =
            await database.MedicineBatches
                .Where(batch =>
                    medicineIds.Contains(batch.MedicineId))
                .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var supplierReturnPrefix =
            await database.PharmacyProfiles
                .AsNoTracking()
                .OrderBy(profile =>
                    profile.CreatedAtUtc)
                .Select(profile =>
                    profile.SupplierReturnPrefix)
                .FirstOrDefaultAsync(
                    cancellationToken)
            ?? "PRT";

        var supplierReturn =
            new SupplierReturn
            {
                ReturnNumber =
                    DocumentNumberGenerator.Generate(
                        supplierReturnPrefix,
                        now),
                PurchaseId = purchase.Id,
                SupplierId = supplier.Id,
                GrossReturnAmount =
                    calculation.GrossReturnAmount,
                PayableReductionAmount =
                    payableReductionAmount,
                SupplierRefundAmount =
                    supplierRefundAmount,
                RefundMethod =
                    supplierRefundAmount > 0
                        ? request.RefundMethod
                        : null,
                Reason = normalizedReason,
                Notes = normalizedNotes,
                RecordedBy = normalizedRecordedBy,
                ReturnedAtUtc = now,
                CreatedAtUtc = now
            };

        var purchaseItemsById =
            purchaseItems.ToDictionary(item => item.Id);

        foreach (var calculatedLine in calculation.Lines)
        {
            var purchaseItem =
                purchaseItemsById[
                    calculatedLine.PurchaseItemId];

            var batch =
                batches.SingleOrDefault(item =>
                    item.MedicineId ==
                        purchaseItem.MedicineId &&
                    item.BatchNumber ==
                        purchaseItem.BatchNumber)
                ?? throw new InvalidOperationException(
                    $"Batch {purchaseItem.BatchNumber} was not found.");

            var totalReturnedQuantity =
                calculatedLine.Quantity +
                calculatedLine.FreeQuantity;

            if (totalReturnedQuantity >
                batch.AvailableQuantity)
            {
                throw new InvalidOperationException(
                    $"Batch {batch.BatchNumber} has only " +
                    $"{batch.AvailableQuantity} available unit(s). " +
                    "Sold, disposed or otherwise unavailable stock " +
                    "cannot be returned to the supplier.");
            }

            batch.AvailableQuantity -=
                totalReturnedQuantity;

            batch.UpdatedAtUtc = now;

            supplierReturn.Items.Add(
                new SupplierReturnItem
                {
                    PurchaseItemId = purchaseItem.Id,
                    MedicineBatchId = batch.Id,
                    Quantity = calculatedLine.Quantity,
                    FreeQuantity =
                        calculatedLine.FreeQuantity,
                    UnitReturnAmount =
                        calculatedLine.UnitReturnAmount,
                    LineReturnAmount =
                        calculatedLine.LineReturnAmount,
                    CreatedAtUtc = now
                });

            database.StockMovements.Add(
                new StockMovement
                {
                    MedicineBatchId = batch.Id,
                    MovementType =
                        StockMovementType.SupplierReturn,
                    QuantityChange =
                        -totalReturnedQuantity,
                    BalanceAfter =
                        batch.AvailableQuantity,
                    ReferenceNumber =
                        supplierReturn.ReturnNumber,
                    Notes =
                        $"Supplier return: {normalizedReason}",
                    CreatedAtUtc = now
                });
        }

        purchase.GrandTotal =
            RoundMoney(
                purchase.GrandTotal -
                calculation.GrossReturnAmount);

        purchase.DueAmount =
            RoundMoney(
                purchase.DueAmount -
                payableReductionAmount);

        purchase.PaidAmount =
            RoundMoney(
                purchase.PaidAmount -
                supplierRefundAmount);

        supplier.CurrentBalance =
            RoundMoney(
                supplier.CurrentBalance -
                payableReductionAmount);

        purchase.UpdatedAtUtc = now;
        supplier.UpdatedAtUtc = now;

        var currentQuantityByItem =
            calculation.Lines.ToDictionary(
                item => item.PurchaseItemId,
                item => item.Quantity);

        var currentFreeQuantityByItem =
            calculation.Lines.ToDictionary(
                item => item.PurchaseItemId,
                item => item.FreeQuantity);

        var allQuantitiesReturned =
            purchase.Items.All(item =>
                previousQuantityByItem.GetValueOrDefault(
                    item.Id) +
                currentQuantityByItem.GetValueOrDefault(
                    item.Id) >=
                item.Quantity &&
                previousFreeQuantityByItem.GetValueOrDefault(
                    item.Id) +
                currentFreeQuantityByItem.GetValueOrDefault(
                    item.Id) >=
                item.FreeQuantity);

        purchase.Status =
            allQuantitiesReturned
                ? PurchaseStatus.Returned
                : PurchaseStatus.Received;

        if (purchase.Status == PurchaseStatus.Returned)
        {
            purchase.GrandTotal = 0;
            purchase.PaidAmount = 0;
            purchase.DueAmount = 0;
        }

        database.SupplierReturns.Add(
            supplierReturn);

        await database.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new SupplierReturnResult(
            supplierReturn.Id,
            supplierReturn.ReturnNumber,
            purchase.PurchaseNumber,
            supplierReturn.GrossReturnAmount,
            supplierReturn.PayableReductionAmount,
            supplierReturn.SupplierRefundAmount,
            purchase.DueAmount,
            supplier.CurrentBalance,
            calculation.Lines.Sum(item =>
                item.Quantity),
            calculation.Lines.Sum(item =>
                item.FreeQuantity),
            calculation.Lines.Count);
    }

    private static Dictionary<Guid, decimal>
        CalculateOriginalLineEntitlements(
            ICollection<PurchaseItem> items,
            decimal originalPurchaseGross)
    {
        var orderedItems =
            items
                .OrderBy(item => item.Id)
                .ToList();

        if (orderedItems.Count == 0)
        {
            throw new InvalidOperationException(
                "The purchase does not contain any items.");
        }

        var originalLineTotal =
            orderedItems.Sum(item =>
                item.LineTotal);

        if (originalLineTotal <= 0)
        {
            if (originalPurchaseGross > 0)
            {
                throw new InvalidOperationException(
                    "The purchase value cannot be allocated across " +
                    "its items.");
            }

            return orderedItems.ToDictionary(
                item => item.Id,
                _ => 0m);
        }

        var result =
            new Dictionary<Guid, decimal>();

        decimal assignedAmount = 0;

        for (var index = 0;
             index < orderedItems.Count;
             index++)
        {
            var item = orderedItems[index];

            decimal entitlement;

            if (index == orderedItems.Count - 1)
            {
                entitlement =
                    RoundMoney(
                        originalPurchaseGross -
                        assignedAmount);
            }
            else
            {
                entitlement =
                    RoundMoney(
                        originalPurchaseGross *
                        item.LineTotal /
                        originalLineTotal);

                assignedAmount =
                    RoundMoney(
                        assignedAmount +
                        entitlement);
            }

            result[item.Id] =
                Math.Max(0, entitlement);
        }

        return result;
    }

    private static void ValidateRequest(
        SupplierReturnRequest request)
    {
        if (request.PurchaseId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid purchase must be selected.");
        }

        if (request.Items is null ||
            request.Items.Count == 0)
        {
            throw new ArgumentException(
                "Select at least one purchased item to return.");
        }

        var duplicateItem =
            request.Items
                .GroupBy(item =>
                    item.PurchaseItemId)
                .FirstOrDefault(group =>
                    group.Count() > 1);

        if (duplicateItem is not null)
        {
            throw new ArgumentException(
                "Each purchased item may be returned only once per " +
                "transaction.");
        }

        foreach (var item in request.Items)
        {
            if (item.PurchaseItemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid purchased item is required.");
            }

            if (item.Quantity < 0 ||
                item.FreeQuantity < 0)
            {
                throw new ArgumentException(
                    "Supplier-return quantities cannot be negative.");
            }

            if (item.Quantity +
                item.FreeQuantity <= 0)
            {
                throw new ArgumentException(
                    "At least one paid or free unit must be returned.");
            }
        }
    }

    private static void ValidateRefundMethod(
        PaymentMethod? refundMethod,
        decimal refundedAmount)
    {
        if (refundedAmount <= 0)
        {
            return;
        }

        if (!refundMethod.HasValue ||
            !Enum.IsDefined(
                typeof(PaymentMethod),
                refundMethod.Value) ||
            refundMethod == PaymentMethod.Due)
        {
            throw new ArgumentException(
                "Select a valid supplier refund method.");
        }
    }

    private static string NormalizeRequired(
        string? value,
        int maximumLength,
        string fieldName)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException(
                $"{fieldName} is required.");
        }

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException(
                $"{fieldName} cannot exceed " +
                $"{maximumLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(
        string? value,
        int maximumLength,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException(
                $"{fieldName} cannot exceed " +
                $"{maximumLength} characters.");
        }

        return normalized;
    }

    private static decimal RoundMoney(decimal amount)
    {
        return Math.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero);
    }
}
