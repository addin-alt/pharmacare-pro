using System.Data;
using Microsoft.EntityFrameworkCore;
using PharmaCarePro.Application.Payments;
using PharmaCarePro.Domain.Entities;
using PharmaCarePro.Web.Data;

namespace PharmaCarePro.Web.Services.Payments;

public sealed class AccountPaymentService(
    IServiceScopeFactory scopeFactory)
{
    public async Task<AccountPaymentResult>
        RecordCustomerPaymentAsync(
            Guid customerId,
            decimal amount,
            PaymentMethod paymentMethod,
            string? referenceNumber,
            string? notes,
            string recordedBy,
            CancellationToken cancellationToken = default)
    {
        ValidateAccountId(customerId, "customer");
        amount = ValidateAmount(amount);
        ValidatePaymentMethod(paymentMethod);

        var normalizedRecordedBy =
            NormalizeRequired(
                recordedBy,
                200,
                "Recorded by");

        var normalizedReference =
            NormalizeOptional(
                referenceNumber,
                100,
                "Reference number");

        var normalizedNotes =
            NormalizeOptional(
                notes,
                500,
                "Notes");

        using var scope = scopeFactory.CreateScope();

        var database =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        await using var transaction =
            await database.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var customer =
            await database.Customers
                .SingleOrDefaultAsync(
                    item => item.Id == customerId,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Customer was not found.");

        var balanceBefore =
            RoundMoney(customer.CurrentBalance);

        ValidateAvailableBalance(
            balanceBefore,
            amount,
            "customer");

        var outstandingSales =
            await database.Sales
                .Where(sale =>
                    sale.CustomerId == customerId &&
                    sale.DueAmount > 0 &&
                    sale.Status != SaleStatus.Cancelled &&
                    sale.Status != SaleStatus.Refunded)
                .OrderBy(sale => sale.SoldAtUtc)
                .ThenBy(sale => sale.Id)
                .ToListAsync(cancellationToken);

        var allocationPlan =
            OldestBalanceAllocator.Allocate(
                outstandingSales.Select(sale =>
                    new OutstandingBalanceCandidate(
                        sale.Id,
                        sale.SoldAtUtc,
                        sale.DueAmount)),
                amount);

        var salesById =
            outstandingSales.ToDictionary(sale => sale.Id);

        var now = DateTime.UtcNow;
        var balanceAfter =
            RoundMoney(balanceBefore - amount);

        var appliedToSales =
            RoundMoney(
                allocationPlan.Sum(allocation =>
                    allocation.Amount));

        var appliedToAccountBalance =
            RoundMoney(amount - appliedToSales);

        var payment =
            new CustomerPayment
            {
                ReceiptNumber =
                    GenerateNumber("CPY", now),
                CustomerId = customer.Id,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                AppliedToSalesAmount = appliedToSales,
                AppliedToAccountBalanceAmount =
                    appliedToAccountBalance,
                PaymentMethod = paymentMethod,
                ReferenceNumber = normalizedReference,
                Notes = normalizedNotes,
                RecordedBy = normalizedRecordedBy,
                ReceivedAtUtc = now,
                CreatedAtUtc = now
            };

        foreach (var allocation in allocationPlan)
        {
            var sale = salesById[allocation.DocumentId];

            sale.PaidAmount =
                RoundMoney(
                    sale.PaidAmount + allocation.Amount);

            sale.DueAmount =
                RoundMoney(
                    sale.DueAmount - allocation.Amount);

            if (sale.DueAmount <= 0)
            {
                sale.DueAmount = 0;
                sale.Status = SaleStatus.Completed;
            }
            else
            {
                sale.Status = SaleStatus.PartiallyPaid;
            }

            payment.Allocations.Add(
                new CustomerPaymentAllocation
                {
                    SaleId = sale.Id,
                    Amount = allocation.Amount,
                    CreatedAtUtc = now
                });
        }

        customer.CurrentBalance = balanceAfter;
        customer.UpdatedAtUtc = now;

        database.CustomerPayments.Add(payment);

        await database.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new AccountPaymentResult(
            payment.Id,
            payment.ReceiptNumber,
            payment.Amount,
            payment.BalanceBefore,
            payment.BalanceAfter,
            payment.AppliedToSalesAmount,
            payment.AppliedToAccountBalanceAmount,
            payment.Allocations.Count);
    }

    public async Task<AccountPaymentResult>
        RecordSupplierPaymentAsync(
            Guid supplierId,
            decimal amount,
            PaymentMethod paymentMethod,
            string? referenceNumber,
            string? notes,
            string recordedBy,
            CancellationToken cancellationToken = default)
    {
        ValidateAccountId(supplierId, "supplier");
        amount = ValidateAmount(amount);
        ValidatePaymentMethod(paymentMethod);

        var normalizedRecordedBy =
            NormalizeRequired(
                recordedBy,
                200,
                "Recorded by");

        var normalizedReference =
            NormalizeOptional(
                referenceNumber,
                100,
                "Reference number");

        var normalizedNotes =
            NormalizeOptional(
                notes,
                500,
                "Notes");

        using var scope = scopeFactory.CreateScope();

        var database =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        await using var transaction =
            await database.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var supplier =
            await database.Suppliers
                .SingleOrDefaultAsync(
                    item => item.Id == supplierId,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "Supplier was not found.");

        var balanceBefore =
            RoundMoney(supplier.CurrentBalance);

        ValidateAvailableBalance(
            balanceBefore,
            amount,
            "supplier");

        var outstandingPurchases =
            await database.Purchases
                .Where(purchase =>
                    purchase.SupplierId == supplierId &&
                    purchase.DueAmount > 0 &&
                    purchase.Status ==
                        PurchaseStatus.Received)
                .OrderBy(purchase =>
                    purchase.PurchaseDateUtc)
                .ThenBy(purchase => purchase.Id)
                .ToListAsync(cancellationToken);

        var allocationPlan =
            OldestBalanceAllocator.Allocate(
                outstandingPurchases.Select(purchase =>
                    new OutstandingBalanceCandidate(
                        purchase.Id,
                        purchase.PurchaseDateUtc,
                        purchase.DueAmount)),
                amount);

        var purchasesById =
            outstandingPurchases.ToDictionary(
                purchase => purchase.Id);

        var now = DateTime.UtcNow;
        var balanceAfter =
            RoundMoney(balanceBefore - amount);

        var appliedToPurchases =
            RoundMoney(
                allocationPlan.Sum(allocation =>
                    allocation.Amount));

        var appliedToAccountBalance =
            RoundMoney(
                amount - appliedToPurchases);

        var payment =
            new SupplierPayment
            {
                PaymentNumber =
                    GenerateNumber("SPY", now),
                SupplierId = supplier.Id,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                AppliedToPurchasesAmount =
                    appliedToPurchases,
                AppliedToAccountBalanceAmount =
                    appliedToAccountBalance,
                PaymentMethod = paymentMethod,
                ReferenceNumber = normalizedReference,
                Notes = normalizedNotes,
                RecordedBy = normalizedRecordedBy,
                PaidAtUtc = now,
                CreatedAtUtc = now
            };

        foreach (var allocation in allocationPlan)
        {
            var purchase =
                purchasesById[allocation.DocumentId];

            purchase.PaidAmount =
                RoundMoney(
                    purchase.PaidAmount +
                    allocation.Amount);

            purchase.DueAmount =
                RoundMoney(
                    purchase.DueAmount -
                    allocation.Amount);

            if (purchase.DueAmount <= 0)
            {
                purchase.DueAmount = 0;
            }

            purchase.UpdatedAtUtc = now;

            payment.Allocations.Add(
                new SupplierPaymentAllocation
                {
                    PurchaseId = purchase.Id,
                    Amount = allocation.Amount,
                    CreatedAtUtc = now
                });
        }

        supplier.CurrentBalance = balanceAfter;
        supplier.UpdatedAtUtc = now;

        database.SupplierPayments.Add(payment);

        await database.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return new AccountPaymentResult(
            payment.Id,
            payment.PaymentNumber,
            payment.Amount,
            payment.BalanceBefore,
            payment.BalanceAfter,
            payment.AppliedToPurchasesAmount,
            payment.AppliedToAccountBalanceAmount,
            payment.Allocations.Count);
    }

    private static void ValidateAccountId(
        Guid accountId,
        string accountType)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                $"A valid {accountType} must be selected.");
        }
    }

    private static decimal ValidateAmount(decimal amount)
    {
        amount = RoundMoney(amount);

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Payment amount must be greater than zero.");
        }

        if (amount > 999_999_999m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Payment amount exceeds the supported limit.");
        }

        return amount;
    }

    private static void ValidatePaymentMethod(
        PaymentMethod paymentMethod)
    {
        if (!Enum.IsDefined(paymentMethod) ||
            paymentMethod == PaymentMethod.Due)
        {
            throw new ArgumentException(
                "Select a valid payment method.");
        }
    }

    private static void ValidateAvailableBalance(
        decimal balance,
        decimal amount,
        string accountType)
    {
        if (balance <= 0)
        {
            throw new InvalidOperationException(
                $"The {accountType} account is already settled.");
        }

        if (amount > balance)
        {
            throw new InvalidOperationException(
                $"Payment cannot exceed the {accountType} " +
                $"balance of ৳{balance:N2}.");
        }
    }

    private static string GenerateNumber(
        string prefix,
        DateTime createdAtUtc)
    {
        var suffix =
            Guid.NewGuid()
                .ToString("N")[..6]
                .ToUpperInvariant();

        return
            $"{prefix}-" +
            $"{createdAtUtc:yyyyMMddHHmmss}-" +
            suffix;
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
