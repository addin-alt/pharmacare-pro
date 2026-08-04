namespace PharmaCarePro.Domain.Entities;

public enum PrescriptionStatus
{
    Active = 1,
    PartiallyDispensed = 2,
    Dispensed = 3,
    Expired = 4,
    Cancelled = 5,
}
