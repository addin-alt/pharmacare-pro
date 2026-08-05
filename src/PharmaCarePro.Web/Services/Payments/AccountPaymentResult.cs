namespace PharmaCarePro.Web.Services.Payments;

public sealed record AccountPaymentResult(
    Guid PaymentId,
    string PaymentNumber,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    decimal AppliedToDocumentsAmount,
    decimal AppliedToAccountBalanceAmount,
    int AllocatedDocumentCount);
