namespace PharmaCarePro.Domain.Entities;

public enum StockMovementType
{
    OpeningStock = 1,
    PurchaseReceipt = 2,
    Sale = 3,
    CustomerReturn = 4,
    SupplierReturn = 5,
    AdjustmentIncrease = 6,
    AdjustmentDecrease = 7,
    TransferIn = 8,
    TransferOut = 9,
    Damage = 10,
    Expiry = 11,
    Disposal = 12,
}
