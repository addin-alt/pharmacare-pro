namespace PharmaCarePro.Web.Services.Returns;

public sealed record SaleReturnResult(
    Guid SaleReturnId,
    string ReturnNumber,
    string InvoiceNumber,
    decimal GrossReturnAmount,
    decimal DueReductionAmount,
    decimal RefundedAmount,
    decimal SaleBalanceAfter,
    decimal? CustomerBalanceAfter,
    int ReturnedQuantity,
    int ReturnedLineCount);
