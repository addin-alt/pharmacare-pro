using System.ComponentModel.DataAnnotations;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.UnitTests;

public sealed class PharmacyProfileValidationTests
{
    [Fact]
    public void Profile_requires_pharmacy_and_branch_names()
    {
        var profile = CreateValidProfile();

        profile.PharmacyName = string.Empty;
        profile.BranchName = string.Empty;

        var results = Validate(profile);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(PharmacyProfile.PharmacyName)));

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(PharmacyProfile.BranchName)));
    }

    [Fact]
    public void Profile_rejects_invalid_currency_and_email()
    {
        var profile = CreateValidProfile();

        profile.CurrencyCode = "BD";
        profile.Email = "not-an-email";

        var results = Validate(profile);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(PharmacyProfile.CurrencyCode)));

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(PharmacyProfile.Email)));
    }

    [Fact]
    public void Valid_profile_passes_validation()
    {
        var profile = CreateValidProfile();

        var results = Validate(profile);

        Assert.Empty(results);
    }

    private static PharmacyProfile CreateValidProfile()
    {
        return new PharmacyProfile
        {
            PharmacyName = "PharmaCare Pro",
            BranchName = "Main Pharmacy",
            Phone = "01700000000",
            Email = "pharmacy@example.com",
            Address = "Dhaka, Bangladesh",
            CurrencyCode = "BDT",
            CurrencySymbol = "৳",
            TimeZoneId = "Asia/Dhaka",
            InvoicePrefix = "PCP",
            PurchasePrefix = "PUR",
            PrescriptionPrefix = "RX",
            ExpiryAlertDays = 90,
            LowStockAlertsEnabled = true,
            ExpiryAlertsEnabled = true
        };
    }

    private static List<ValidationResult> Validate(
        PharmacyProfile profile)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            profile,
            new ValidationContext(profile),
            results,
            validateAllProperties: true);

        return results;
    }
}
