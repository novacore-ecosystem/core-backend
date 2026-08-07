using NovaCore.Shipping.Domain.Regexes;

namespace NovaCore.Shipping.Domain.ValueObjects;

/// <summary>Human-readable business identifier of a Shipment (SHP-yyyyMMdd-XXXX), mirroring Order's OrderNumber.</summary>
public sealed class ShipmentNumber : StringValueObject
{
    private ShipmentNumber(string val) : base(val) { }

    public static ShipmentNumber Create()
    {
        var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        return new ShipmentNumber($"SHP-{DateTime.UtcNow:yyyyMMdd}-{suffix}");
    }

    public static ShipmentNumber Create(string val)
    {
        var normalized = val?.Trim().ToUpperInvariant()
            ?? throw ExceptionFactory.InvalidFormat("ShipmentNumber is not valid.");

        if (!IsValid(normalized))
            throw ExceptionFactory.InvalidFormat("ShipmentNumber is not valid.");

        return new ShipmentNumber(normalized);
    }

    public static bool IsValid(string? val)
        => val.IsNotNullOrWhiteSpace()
            && val!.Length <= 20
            && ShippingRegexes.ShipmentNumber().IsMatch(val);
}
