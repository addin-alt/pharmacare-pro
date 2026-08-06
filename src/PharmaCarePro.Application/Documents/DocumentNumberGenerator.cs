using System.Security.Cryptography;

namespace PharmaCarePro.Application.Documents;

public static class DocumentNumberGenerator
{
    private const int MaximumSuffix = 999_999;

    public static string Generate(
        string prefix,
        DateTime utcDateTime)
    {
        var suffix =
            RandomNumberGenerator.GetInt32(
                MaximumSuffix + 1);

        return Generate(
            prefix,
            utcDateTime,
            suffix);
    }

    public static string Generate(
        string prefix,
        DateTime utcDateTime,
        int suffix)
    {
        var normalizedPrefix =
            NormalizePrefix(prefix);

        if (suffix is < 0 or > MaximumSuffix)
        {
            throw new ArgumentOutOfRangeException(
                nameof(suffix),
                suffix,
                "Document-number suffix must be " +
                "between 0 and 999999.");
        }

        return
            $"{normalizedPrefix}-" +
            $"{utcDateTime:yyyyMMddHHmmss}-" +
            $"{suffix:D6}";
    }

    public static string NormalizePrefix(
        string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        var normalized =
            prefix.Trim().ToUpperInvariant();

        if (normalized.Length is < 2 or > 12)
        {
            throw new ArgumentException(
                "Document prefix must contain " +
                "between 2 and 12 characters.",
                nameof(prefix));
        }

        var containsInvalidCharacter =
            normalized.Any(character =>
                !IsAsciiLetterOrDigit(character) &&
                character != '-');

        if (containsInvalidCharacter)
        {
            throw new ArgumentException(
                "Document prefix may contain only " +
                "letters, numbers and hyphens.",
                nameof(prefix));
        }

        return normalized;
    }

    private static bool IsAsciiLetterOrDigit(
        char character)
    {
        return
            character is >= 'A' and <= 'Z' ||
            character is >= '0' and <= '9';
    }
}
