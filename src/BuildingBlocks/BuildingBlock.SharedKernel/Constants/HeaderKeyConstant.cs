namespace NovaCore.BuildingBlock.SharedKernel.Constants;

public static class HeaderKeyConstant
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string TenantId = "X-Tenant-Id";
    public const string ClientVersion = "X-Client-Version";
    public const string DeviceId = "X-Device-Id";
    public const string IdempotencyKey = "Idempotency-Key";

    /// <summary>Carries a TenantClient's PublicKey on the pre-authentication Login request, so
    /// tenant context can be resolved before username/password are checked (see
    /// docs/services/auth-service.md, Phase 2). Distinct from TenantId above: that one carries a
    /// resolved TenantId (unused today), this one carries an opaque public credential that still
    /// needs resolving - the two are never interchangeable.</summary>
    public const string TenantClientKey = "X-Tenant-Client-Key";

    /// <summary>Carries an App's stable Code on Login/Register/RefreshToken - the pre-authentication
    /// boundary where an App identifier must be resolved and validated before a token exists. Once
    /// a token is issued, the resolved App id travels as the AppClaimTypes.AppId claim instead -
    /// authenticated requests never send this header. Same relationship TenantClientKey has to
    /// AppClaimTypes.TenantId.</summary>
    public const string AppKey = "X-App-Key";

    /// <summary>
    /// Reuses the standard HTTP header rather than inventing a custom one - the frontend already
    /// sends `Accept-Language` on every request (see NovaCoreUI's shared Axios client), so no
    /// frontend change is needed for this to start flowing.
    /// </summary>
    public const string Locale = "Accept-Language";
}
