using System.Data;
using Microsoft.EntityFrameworkCore;
using PharmaCarePro.Application.Documents;
using PharmaCarePro.Application.Returns;
using PharmaCarePro.Domain.Entities;
using PharmaCarePro.Web.Data;

namespace PharmaCarePro.Web.Services.Returns;

public sealed class SaleReturnService(
    IServiceScopeFactory scopeFactory)
{
    public async Task<SaleReturnResult> RecordAsync(
        SaleReturnRequest request,
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

        var sale =
            await database.Sales
                .Include(item => item.Customer)
                .Include(item => item.Items)
                    .ThenInclude(item => item.MedicineBatch)
                .Include(item => item.Returns)
                    .ThenInclude(item => item.Items)
                .SingleOrDefaultAsync(
                    item => item.Id == request.SaleId,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Sale was not found.");

        if (sale.Status == SaleStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "A cancelled sale cannot be returned.");
        }

        if (sale.Status == SaleStatus.Refunded ||
            sale.GrandTotal <= 0)
        {
            throw new InvalidOperationException(
                "This sale has already been fully returned.");
        }

        var requestedItems =
            request.Items.ToDictionary(
                item => item.SaleItemId);

        var saleItems =
            sale.Items
                .Where(item =>
                    requestedItems.ContainsKey(item.Id))
                .ToList();

        if (saleItems.Count != requestedItems.Count)
        {
            throw new InvalidOperationException(
                "One or more selected items do not belong to this sale.");
        }

        var originalSaleGross =
            RoundMoney(
                sale.GrandTotal +
                sale.Returns.Sum(item =>
                    item.GrossReturnAmount));

        var lineEntitlements =
            CalculateOriginalLineEntitlements(
                sale.Items,
                originalSaleGross);

        var previousQuantityByItem =
            sale.Returns
                .SelectMany(item => item.Items)
                .GroupBy(item => item.SaleItemId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(item => item.Quantity));

        var previousAmountByItem =
            sale.Returns
                .SelectMany(item => item.Items)
                .GroupBy(item => item.SaleItemId)
                .ToDictionary(
                    group => group.Key,
                    group => RoundMoney(
                        group.Sum(item =>
                            item.LineRefundAmount)));

        var calculation =
            SaleReturnCalculator.Calculate(
                saleItems.Select(item =>
                    new SaleReturnLineCandidate(
                        item.Id,
                        item.Quantity,
                        previousQuantityByItem.GetValueOrDefault(
                            item.Id),
                        lineEntitlements[item.Id],
                        previousAmountByItem.GetValueOrDefault(
                            item.Id),
                        requestedItems[item.Id].Quantity)));

        if (calculation.GrossReturnAmount >
            sale.GrandTotal)
        {
            throw new InvalidOperationException(
                "Return value cannot exceed the remaining sale value.");
        }

        var dueReductionAmount =
            RoundMoney(
                Math.Min(
                    sale.DueAmount,
                    calculation.GrossReturnAmount));

        var refundedAmount =
            RoundMoney(
                calculation.GrossReturnAmount -
                dueReductionAmount);

        ValidateRefundMethod(
            request.RefundMethod,
            refundedAmount);

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

        var now = DateTime.UtcNow;

        var saleReturnPrefix =
            await database.PharmacyProfiles
                .AsNoTracking()
                .OrderBy(profile =>
                    profile.CreatedAtUtc)
                .Select(profile =>
                    profile.SaleReturnPrefix)
                .FirstOrDefaultAsync(
                    cancellationToken)
            ?? "SRT";

        var saleReturn =
            new SaleReturn
            {
                ReturnNumber =
                    DocumentNumberGenerator.Generate(
                        saleReturnPrefix,
                        now),
                SaleId = sale.Id,
                CustomerId = sale.CustomerId,
                GrossReturnAmount =
                    calculation.GrossReturnAmount,
                DueReductionAmount =
                    dueReductionAmount,
                RefundedAmount = refundedAmount,
                RefundMethod =
                    refundedAmount > 0
                        ? request.RefundMethod
                        : null,
                Reason = normalizedReason,
                Notes = normalizedNotes,
                RecordedBy = normalizedRecordedBy,
                ReturnedAtUtc = now,
                CreatedAtUtc = now
            };

        var saleItemsById =
            saleItems.ToDictionary(item => item.Id);

        foreach (var calculatedLine in calculation.Lines)
        {
            var saleItem =
                saleItemsById[calculatedLine.SaleItemId];

            var requestedItem =
                requestedItems[calculatedLine.SaleItemId];

            var batch =
                saleItem.MedicineBatch
                ?? throw new InvalidOperationException(
                    $"Batch {saleItem.BatchNumber} was not found.");

            saleReturn.Items.Add(
                new SaleReturnItem
                {
                    SaleItemId = saleItem.Id,
                    MedicineBatchId = batch.Id,
                    Quantity = calculatedLine.Quantity,
                    UnitRefundAmount =
                        calculatedLine.UnitRefundAmount,
                    LineRefundAmount =
                        calculatedLine.LineRefundAmount,
                    StockAction =
                        requestedItem.StockAction,
                    CreatedAtUtc = now
                });

            ApplyInventoryAction(
                database,
                batch,
                saleReturn.ReturnNumber,
                normalizedReason,
                calculatedLine.Quantity,
                requestedItem.StockAction,
                now);
        }

        sale.GrandTotal =
            RoundMoney(
                sale.GrandTotal -
                calculation.GrossReturnAmount);

        sale.DueAmount =
            RoundMoney(
                sale.DueAmount -
                dueReductionAmount);

        sale.PaidAmount =
            RoundMoney(
                sale.PaidAmount -
                refundedAmount);

        if (sale.GrandTotal <= 0)
        {
            sale.GrandTotal = 0;
            sale.PaidAmount = 0;
            sale.DueAmount = 0;
            sale.Status = SaleStatus.Refunded;
        }
        else if (sale.DueAmount > 0)
        {
            sale.Status =
                sale.PaidAmount > 0
                    ? SaleStatus.PartiallyPaid
                    : SaleStatus.Due;
        }
        else
        {
            sale.Status = SaleStatus.Completed;
        }

        decimal? customerBalanceAfter = null;

        if (dueReductionAmount > 0 &&
            sale.CustomerId.HasValue)
        {
            var customer =
                sale.Customer
                ?? throw new InvalidOperationException(
                    "The customer account linked to this sale was " +
                    "not found.");

            if (dueReductionAmount >
                customer.CurrentBalance)
            {
                throw new InvalidOperationException(
                    "The return due reduction exceeds the customer " +
                    "account balance.");
            }

            customer.CurrentBalance =
                RoundMoney(
                    customer.CurrentBalance -
                    dueReductionAmount);

            customer.UpdatedAtUtc = now;
            customerBalanceAfter = customer.CurrentBalance;
        }
        else if (sale.Customer is not null)
        {
            customerBalanceAfter =
                sale.Customer.CurrentBalance;
        }

        database.SaleReturns.Add(saleReturn);

        await database.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new SaleReturnResult(
            saleReturn.Id,
            saleReturn.ReturnNumber,
            sale.InvoiceNumber,
            saleReturn.GrossReturnAmount,
            saleReturn.DueReductionAmount,
            saleReturn.RefundedAmount,
            sale.DueAmount,
            customerBalanceAfter,
            calculation.Lines.Sum(item => item.Quantity),
            calculation.Lines.Count);
    }

    private static Dictionary<Guid, decimal>
        CalculateOriginalLineEntitlements(
            ICollection<SaleItem> items,
            decimal originalSaleGross)
    {
        var orderedItems =
            items
                .OrderBy(item => item.Id)
                .ToList();

        if (orderedItems.Count == 0)
        {
            throw new InvalidOperationException(
                "The sale does not contain any items.");
        }

        var originalLineTotal =
            orderedItems.Sum(item => item.LineTotal);

        if (originalLineTotal <= 0)
        {
            throw new InvalidOperationException(
                "The sale items have no refundable value.");
        }

        var result = new Dictionary<Guid, decimal>();
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
                        originalSaleGross -
                        assignedAmount);
            }
            else
            {
                entitlement =
                    RoundMoney(
                        originalSaleGross *
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

    private static void ApplyInventoryAction(
        ApplicationDbContext database,
        MedicineBatch batch,
        string returnNumber,
        string reason,
        int quantity,
        ReturnStockAction stockAction,
        DateTime createdAtUtc)
    {
        var balanceBefore = batch.AvailableQuantity;
        var temporaryBalance = balanceBefore + quantity;

        if (temporaryBalance >
            batch.ReceivedQuantity + batch.FreeQuantity)
        {
            throw new InvalidOperationException(
                $"Returning {quantity} unit(s) to batch " +
                $"{batch.BatchNumber} would exceed its original " +
                "received quantity.");
        }

        database.StockMovements.Add(
            new StockMovement
            {
                MedicineBatchId = batch.Id,
                MovementType =
                    StockMovementType.CustomerReturn,
                QuantityChange = quantity,
                BalanceAfter = temporaryBalance,
                ReferenceNumber = returnNumber,
                Notes =
                    $"Customer return: {reason}",
                CreatedAtUtc = createdAtUtc
            });

        switch (stockAction)
        {
            case ReturnStockAction.Restock:
                batch.AvailableQuantity = temporaryBalance;
                break;

            case ReturnStockAction.Quarantine:
                database.StockMovements.Add(
                    new StockMovement
                    {
                        MedicineBatchId = batch.Id,
                        MovementType =
                            StockMovementType.AdjustmentDecrease,
                        QuantityChange = -quantity,
                        BalanceAfter = balanceBefore,
                        ReferenceNumber = returnNumber,
                        Notes =
                            "Returned units held outside saleable " +
                            "inventory for quarantine.",
                        CreatedAtUtc = createdAtUtc
                    });
                break;

            case ReturnStockAction.Dispose:
                database.StockMovements.Add(
                    new StockMovement
                    {
                        MedicineBatchId = batch.Id,
                        MovementType =
                            StockMovementType.Disposal,
                        QuantityChange = -quantity,
                        BalanceAfter = balanceBefore,
                        ReferenceNumber = returnNumber,
                        Notes =
                            "Returned units marked for disposal.",
                        CreatedAtUtc = createdAtUtc
                    });
                break;

            default:
                throw new ArgumentException(
                    "Select a valid stock action.");
        }

        batch.UpdatedAtUtc = createdAtUtc;
    }

    private static void ValidateRequest(
        SaleReturnRequest request)
    {
        if (request.SaleId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid sale must be selected.");
        }

        if (request.Items is null ||
            request.Items.Count == 0)
        {
            throw new ArgumentException(
                "Select at least one item to return.");
        }

        var duplicateItem =
            request.Items
                .GroupBy(item => item.SaleItemId)
                .FirstOrDefault(group => group.Count() > 1);

        if (duplicateItem is not null)
        {
            throw new ArgumentException(
                "Each sale item may be returned only once per " +
                "transaction.");
        }

        foreach (var item in request.Items)
        {
            if (item.SaleItemId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid sale item is required.");
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Return quantity must be greater than zero.");
            }

            if (!Enum.IsDefined(
                    typeof(ReturnStockAction),
                    item.StockAction))
            {
                throw new ArgumentException(
                    "Select a valid stock action.");
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
                "Select a valid refund method.");
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
