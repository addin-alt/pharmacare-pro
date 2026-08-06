using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class PharmacyDocumentPrefixValidationTests
{
    [Fact]
    public void OperationalPrefixes_HaveExpectedDefaults()
    {
        var profile = new PharmacyProfile();

        Assert.Equal(
            "CPY",
            profile.CustomerPaymentPrefix);

        Assert.Equal(
            "SPY",
            profile.SupplierPaymentPrefix);

        Assert.Equal(
            "SRT",
            profile.SaleReturnPrefix);

        Assert.Equal(
            "PRT",
            profile.SupplierReturnPrefix);
    }

    [Fact]
    public void OperationalPrefixes_AcceptValidValues()
    {
        var profile = new PharmacyProfile
        {
            CustomerPaymentPrefix = "MAIN-CPY",
            SupplierPaymentPrefix = "MAIN-SPY",
            SaleReturnPrefix = "MAIN-SRT",
            SupplierReturnPrefix = "MAIN-PRT"
        };

        var validationResults =
            new List<ValidationResult>();

        var isValid =
            Validator.TryValidateObject(
                profile,
                new ValidationContext(profile),
                validationResults,
                validateAllProperties: true);

        Assert.True(
            isValid,
            string.Join(
                Environment.NewLine,
                validationResults.Select(
                    result => result.ErrorMessage)));
    }

    [Theory]
    [InlineData(
        nameof(PharmacyProfile.CustomerPaymentPrefix))]
    [InlineData(
        nameof(PharmacyProfile.SupplierPaymentPrefix))]
    [InlineData(
        nameof(PharmacyProfile.SaleReturnPrefix))]
    [InlineData(
        nameof(PharmacyProfile.SupplierReturnPrefix))]
    public void OperationalPrefixes_RejectInvalidCharacters(
        string propertyName)
    {
        var profile = new PharmacyProfile();

        var property =
            typeof(PharmacyProfile)
                .GetProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Property {propertyName} was not found.");

        property.SetValue(profile, "BAD/PREFIX");

        var validationResults =
            new List<ValidationResult>();

        var isValid =
            Validator.TryValidateObject(
                profile,
                new ValidationContext(profile),
                validationResults,
                validateAllProperties: true);

        Assert.False(isValid);

        Assert.Contains(
            validationResults,
            result =>
                result.MemberNames.Contains(
                    propertyName));
    }
}
