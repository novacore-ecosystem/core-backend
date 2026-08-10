namespace NovaCore.BuildingBlock.SharedKernel.Constants;

public static class HeaderKeyConstant
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string TenantId = "X-Tenant-Id";
    public const string ClientVersion = "X-Client-Version";
    public const string DeviceId = "X-Device-Id";
    public const string IdempotencyKey = "Idempotency-Key";

    /// <summary>
    /// Reuses the standard HTTP header rather than inventing a custom one - the frontend already
    /// sends `Accept-Language` on every request (see NovaCoreUI's shared Axios client), so no
    /// frontend change is needed for this to start flowing.
    /// </summary>
    public const string Locale = "Accept-Language";
}
