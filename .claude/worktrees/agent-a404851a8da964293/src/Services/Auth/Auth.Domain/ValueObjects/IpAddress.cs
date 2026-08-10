using SystemIPAddress = System.Net.IPAddress;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// Format-validated IPv4/IPv6 address string, recorded on Session and LoginHistory. Kept local
/// to Auth.Domain for now (YAGNI) - promote to BuildingBlock.Domain if another service needs IP logging.
/// </summary>
public sealed class IpAddress : StringValueObject
{
    private IpAddress(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out IpAddress? ipAddress)
    {
        if (GetValidationError(value) is not null)
        {
            ipAddress = null;
            return false;
        }

        ipAddress = new IpAddress(value!.Trim());
        return true;
    }

    public static IpAddress Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new IpAddress(value.Trim());
    }

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("IP address cannot be empty.");

        if (!SystemIPAddress.TryParse(value.Trim(), out _))
            return ExceptionFactory.InvalidFormat("IP address is not a valid IPv4 or IPv6 address.");

        return null;
    }
}
