using NovaCore.Shipping.Domain.Regexes;

namespace NovaCore.Shipping.Domain.ValueObjects;

/// <summary>Human-readable business identifier of one Transportation attempt (TRN-yyyyMMdd-XXXX).</summary>
public sealed class TransportationNumber : StringValueObject
{
    private TransportationNumber(string val) : base(val) { }

    public static TransportationNumber Create()
    {
        var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        return new TransportationNumber($"TRN-{DateTime.UtcNow:yyyyMMdd}-{suffix}");
    }

    public static TransportationNumber Create(string val)
    {
        var normalized = val?.Trim().ToUpperInvariant()
            ?? throw ExceptionFactory.InvalidFormat("TransportationNumber is not valid.");

        if (!IsValid(normalized))
            throw ExceptionFactory.InvalidFormat("TransportationNumber is not valid.");

        return new TransportationNumber(normalized);
    }

    public static bool IsValid(string? val)
        => val.IsNotNullOrWhiteSpace()
            && val!.Length <= 20
            && ShippingRegexes.TransportationNumber().IsMatch(val);
}
