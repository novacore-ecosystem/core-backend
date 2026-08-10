namespace NovaCore.Payment.Domain.ValueObjects;

/// <summary>ISO-4217 3-letter currency code (e.g. USD, EUR, VND).</summary>
public sealed class Currency : StringValueObject
{
    private Currency(string val) : base(val) { }

    public static Currency Create(string val)
    {
        var normalized = val?.Trim().ToUpperInvariant()
            ?? throw ExceptionFactory.RequiredField("Currency code cannot be empty.");

        if (!IsValid(normalized))
            throw ExceptionFactory.InvalidFormat("Currency code must be a 3-letter ISO-4217 code (e.g. USD).");

        return new Currency(normalized);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace()
            && val.Length == 3
            && val.All(char.IsAsciiLetterUpper);
}
