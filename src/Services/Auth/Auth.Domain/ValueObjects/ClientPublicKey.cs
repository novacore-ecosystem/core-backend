using System.Security.Cryptography;

namespace NovaCore.Auth.Domain.ValueObjects;

/// <summary>
/// High-entropy identifier a client presents, unauthenticated, to resolve which Tenant it belongs
/// to before login (see TenantClient). Never derived from TenantId/TenantCode and never a secret -
/// safe to ship in client-side code. Always system-generated via <see cref="Generate"/>; Create/
/// TryCreate only reconstruct a value already generated and read back from storage.
/// </summary>
public sealed class ClientPublicKey : StringValueObject
{
    private const int EntropyBytes = 32;
    private const int Length = EntropyBytes * 2;

    private ClientPublicKey(string value) : base(value) { }

    public static bool IsValid(string? value) => GetValidationError(value) is null;

    public static bool TryCreate(string? value, out ClientPublicKey? publicKey)
    {
        if (GetValidationError(value) is not null)
        {
            publicKey = null;
            return false;
        }

        publicKey = new ClientPublicKey(value!);
        return true;
    }

    public static ClientPublicKey Create(string value)
    {
        var error = GetValidationError(value);
        if (error is not null)
            throw error;

        return new ClientPublicKey(value);
    }

    /// <summary>32 bytes (256 bits) of CSPRNG output, lowercase-hex-encoded so the value needs no
    /// escaping wherever a client transmits it (URL, header, query string).</summary>
    public static ClientPublicKey Generate() => new(Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(EntropyBytes)));

    private static InvalidArgumentException? GetValidationError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ExceptionFactory.RequiredField("Client public key cannot be empty.");

        if (value.Length != Length)
            return ExceptionFactory.InvalidFormat($"Client public key must be exactly {Length} hex characters.");

        return null;
    }
}
