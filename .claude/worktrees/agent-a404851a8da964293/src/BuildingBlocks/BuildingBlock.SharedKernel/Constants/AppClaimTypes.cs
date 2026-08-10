namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Custom claim type keys used across the platform. Named "App" to avoid colliding with
/// System.Security.Claims.ClaimTypes. Add new claim type keys here as they're introduced - this
/// class defines keys only, nothing reads or evaluates them.
/// </summary>
public static class AppClaimTypes
{
    /// <summary>Claim type carrying one permission key per claim - see Permissions.</summary>
    public const string Permission = "permission";

    /// <summary>Which Tenant the current user belongs to. Not emitted by the token issuer yet -
    /// reading this claim is wired end-to-end so it starts flowing the moment issuance catches up.</summary>
    public const string TenantId = "tenant_id";

    /// <summary>Every Scope (within a Tenant) the current user is allowed to act under - already
    /// expanded to include descendant scopes at token-issuance time, so business services never
    /// compute the Scope hierarchy themselves. Emitted as one claim per accessible Scope (the
    /// standard inbound-JWT shape for an array claim), not a single value. Not emitted by the
    /// token issuer yet - see TenantId.</summary>
    public const string ScopeIds = "scope_ids";
}
