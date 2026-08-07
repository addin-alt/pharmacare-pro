namespace PharmaCarePro.Application.Prescriptions;

public sealed record PrescriptionDispensingLine(
    Guid PrescriptionItemId,
    Guid MedicineId,
    int QuantityPrescribed,
    int QuantityDispensed,
    int QuantityRequested);

public sealed record PrescriptionDispensingPlan(
    bool IsFullyDispensed,
    IReadOnlyDictionary<Guid, int>
        NewDispensedQuantities);

public static class PrescriptionDispensingCalculator
{
    public static PrescriptionDispensingPlan Calculate(
        IEnumerable<PrescriptionDispensingLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var preparedLines = lines.ToList();

        if (preparedLines.Count == 0)
        {
            throw new ArgumentException(
                "At least one prescription item is required.",
                nameof(lines));
        }

        var duplicateItem =
            preparedLines
                .GroupBy(line =>
                    line.PrescriptionItemId)
                .FirstOrDefault(group =>
                    group.Count() > 1);

        if (duplicateItem is not null)
        {
            throw new ArgumentException(
                "Each prescription item may appear only once.",
                nameof(lines));
        }

        foreach (var line in preparedLines)
        {
            ValidateLine(line);
        }

        if (!preparedLines.Any(line =>
                line.QuantityRequested > 0))
        {
            throw new InvalidOperationException(
                "At least one prescribed medicine must be " +
                "selected for dispensing.");
        }

        var newDispensedQuantities =
            new Dictionary<Guid, int>();

        foreach (var line in preparedLines)
        {
            var remainingQuantity =
                line.QuantityPrescribed -
                line.QuantityDispensed;

            if (line.QuantityRequested >
                remainingQuantity)
            {
                throw new InvalidOperationException(
                    "The requested dispensing quantity exceeds " +
                    "the remaining prescribed quantity.");
            }

            newDispensedQuantities[
                line.PrescriptionItemId] =
                line.QuantityDispensed +
                line.QuantityRequested;
        }

        var isFullyDispensed =
            preparedLines.All(line =>
                newDispensedQuantities[
                    line.PrescriptionItemId] >=
                line.QuantityPrescribed);

        return new PrescriptionDispensingPlan(
            isFullyDispensed,
            newDispensedQuantities);
    }

    private static void ValidateLine(
        PrescriptionDispensingLine line)
    {
        if (line.PrescriptionItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid prescription-item identifier " +
                "is required.");
        }

        if (line.MedicineId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid medicine identifier is required.");
        }

        if (line.QuantityPrescribed <= 0)
        {
            throw new ArgumentException(
                "Prescribed quantity must be greater than zero.");
        }

        if (line.QuantityDispensed < 0)
        {
            throw new ArgumentException(
                "Dispensed quantity cannot be negative.");
        }

        if (line.QuantityDispensed >
            line.QuantityPrescribed)
        {
            throw new ArgumentException(
                "Dispensed quantity cannot exceed the " +
                "prescribed quantity.");
        }

        if (line.QuantityRequested < 0)
        {
            throw new ArgumentException(
                "Requested dispensing quantity cannot be negative.");
        }
    }
}
