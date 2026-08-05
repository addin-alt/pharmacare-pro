using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.Web.Services.Returns;

public sealed record SaleReturnItemRequest(
    Guid SaleItemId,
    int Quantity,
    ReturnStockAction StockAction);

public sealed record SaleReturnRequest(
    Guid SaleId,
    IReadOnlyList<SaleReturnItemRequest> Items,
    string Reason,
    PaymentMethod? RefundMethod,
    string? Notes,
    string RecordedBy);
