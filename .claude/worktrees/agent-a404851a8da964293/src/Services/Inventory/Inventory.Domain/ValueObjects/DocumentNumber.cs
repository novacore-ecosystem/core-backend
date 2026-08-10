namespace NovaCore.Inventory.Domain.ValueObjects;

/// <summary>
/// Encapsulates document number generation and formatting.
/// Centralizes numbering strategy so future changes (sequence, prefix, etc.) require changes only here.
/// </summary>
public sealed class DocumentNumber : ValueObject
{
    public string Value { get; }

    private DocumentNumber(string value)
    {
        if (value.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Document number cannot be empty.");

        Value = value.Trim();
    }

    /// <summary>Generates a new unique document number using current strategy (16-char hex).</summary>
    public static DocumentNumber Generate()
    {
        var number = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpperInvariant();
        return new DocumentNumber(number);
    }

    /// <summary>Creates a document number from an existing string (e.g. from database or external system).</summary>
    public static DocumentNumber Create(string value) => new(value);

    public override string ToString() => Value;

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
