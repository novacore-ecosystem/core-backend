using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - one linked external/OAuth provider login. An Account can link
/// several providers, each with its own lifecycle (link/unlink), so this is a full entity rather
/// than a value object.
/// </summary>
public sealed class ExternalIdentity : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public ExternalProvider Provider { get; private set; }
    public string ProviderUserId { get; private set; } = string.Empty;
    public DateTime LinkedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ExternalIdentity() { }

    public static ExternalIdentity Link(
        Guid accountId,
        ExternalProvider provider,
        string providerUserId)
    {
        ValidateProviderUserId(providerUserId);

        return new ExternalIdentity
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Provider = provider,
            ProviderUserId = providerUserId,
            LinkedAt = DateTime.UtcNow,
        };
    }

    #region Lifecycle

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    #endregion

    public static bool IsValidProviderUserId(string? providerUserId) => providerUserId.IsNotNullOrWhiteSpace();

    private static void ValidateProviderUserId(string providerUserId)
    {
        if (!IsValidProviderUserId(providerUserId))
            throw ExceptionFactory.RequiredField("Provider user id cannot be empty.");
    }
}
