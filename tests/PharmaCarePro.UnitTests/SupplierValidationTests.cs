using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class SupplierValidationTests
{
    [Fact]
    public void Supplier_requires_code_and_name()
    {
        var supplier = new Supplier();

        var results = Validate(supplier);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Supplier.SupplierCode)));

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Supplier.Name)));
    }

    [Fact]
    public void Supplier_rejects_invalid_email()
    {
        var supplier = new Supplier
        {
            SupplierCode = "SUP-0001",
            Name = "Test Supplier",
            Email = "not-an-email"
        };

        var results = Validate(supplier);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Supplier.Email)));
    }

    [Fact]
    public void Valid_supplier_passes_validation()
    {
        var supplier = new Supplier
        {
            SupplierCode = "SUP-0001",
            Name = "Beximco Pharmaceuticals",
            ContactPerson = "Sales Department",
            Phone = "01700000000",
            Email = "sales@example.com",
            OpeningBalance = 0,
            CurrentBalance = 0
        };

        var results = Validate(supplier);

        Assert.Empty(results);
    }

    private static List<ValidationResult> Validate(
        Supplier supplier)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            supplier,
            new ValidationContext(supplier),
            results,
            validateAllProperties: true);

        return results;
    }
}
