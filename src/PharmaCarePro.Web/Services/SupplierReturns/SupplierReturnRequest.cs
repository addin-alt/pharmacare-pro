using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.Web.Services.SupplierReturns;

public sealed record SupplierReturnItemRequest(
    Guid PurchaseItemId,
    int Quantity,
    int FreeQuantity);

public sealed record SupplierReturnRequest(
    Guid PurchaseId,
    IReadOnlyList<SupplierReturnItemRequest> Items,
    string Reason,
    PaymentMethod? RefundMethod,
    string? Notes,
    string RecordedBy);
