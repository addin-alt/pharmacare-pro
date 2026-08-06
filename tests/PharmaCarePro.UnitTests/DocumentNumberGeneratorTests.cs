using PharmaCarePro.Application.Documents;

namespace PharmaCarePro.UnitTests;

public sealed class DocumentNumberGeneratorTests
{
    [Fact]
    public void Generate_NormalizesPrefixAndFormatsNumber()
    {
        var timestamp =
            new DateTime(
                2026,
                8,
                7,
                1,
                2,
                3,
                DateTimeKind.Utc);

        var number =
            DocumentNumberGenerator.Generate(
                " pcp ",
                timestamp,
                42);

        Assert.Equal(
            "PCP-20260807010203-000042",
            number);
    }

    [Fact]
    public void Generate_AllowsHyphenatedPrefix()
    {
        var timestamp =
            new DateTime(
                2026,
                8,
                7,
                1,
                2,
                3,
                DateTimeKind.Utc);

        var number =
            DocumentNumberGenerator.Generate(
                "main-rx",
                timestamp,
                123456);

        Assert.Equal(
            "MAIN-RX-20260807010203-123456",
            number);
    }

    [Fact]
    public void NormalizePrefix_RejectsShortPrefix()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                DocumentNumberGenerator
                    .NormalizePrefix("X"));

        Assert.Contains(
            "between 2 and 12",
            exception.Message);
    }

    [Fact]
    public void NormalizePrefix_RejectsInvalidCharacters()
    {
        var exception =
            Assert.Throws<ArgumentException>(() =>
                DocumentNumberGenerator
                    .NormalizePrefix("RX/MAIN"));

        Assert.Contains(
            "letters, numbers and hyphens",
            exception.Message);
    }

    [Fact]
    public void Generate_RejectsSuffixOutsideRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DocumentNumberGenerator.Generate(
                "PCP",
                DateTime.UtcNow,
                1_000_000));
    }
}
