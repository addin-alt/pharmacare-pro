using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class CustomerValidationTests
{
    [Fact]
    public void Customer_requires_code_and_name()
    {
        var customer = new Customer();

        var results = Validate(customer);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Customer.CustomerCode)));

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Customer.Name)));
    }

    [Fact]
    public void Customer_rejects_invalid_email()
    {
        var customer = new Customer
        {
            CustomerCode = "CUS-0001",
            Name = "Test Customer",
            Email = "not-an-email"
        };

        var results = Validate(customer);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(Customer.Email)));
    }

    [Fact]
    public void Valid_customer_passes_validation()
    {
        var customer = new Customer
        {
            CustomerCode = "CUS-0001",
            Name = "Walk-in Customer",
            Phone = "01700000001",
            Email = "customer@example.com",
            CurrentBalance = 0,
            LoyaltyPoints = 0,
            IsActive = true
        };

        var results = Validate(customer);

        Assert.Empty(results);
    }

    private static List<ValidationResult> Validate(
        Customer customer)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            customer,
            new ValidationContext(customer),
            results,
            validateAllProperties: true);

        return results;
    }
}
