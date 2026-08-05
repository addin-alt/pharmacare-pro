namespace PharmaCarePro.Web.Services.SupplierReturns;

public sealed record SupplierReturnResult(
    Guid SupplierReturnId,
    string ReturnNumber,
    string PurchaseNumber,
    decimal GrossReturnAmount,
    decimal PayableReductionAmount,
    decimal SupplierRefundAmount,
    decimal PurchaseDueAfter,
    decimal SupplierBalanceAfter,
    int ReturnedQuantity,
    int ReturnedFreeQuantity,
    int ReturnedLineCount);
