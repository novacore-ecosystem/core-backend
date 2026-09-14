using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.BuildingBlock.Domain.Abstractions;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Marks an Account as a member of an App - the assignment authentication/authorization checks
/// against. Pure existence-mapping (no history/lifecycle beyond the pairing itself), so it stays
/// BaseEntity with a composite (AccountId, AppId) key instead of a surrogate Id, same shape as
/// AccountRole.
/// </summary>
public sealed class AccountApp : BaseEntity, IAuditable
{
    public Guid AccountId { get; private set; }
    public Guid AppId { get; private set; }

    public Account? Account { get; set; }
    public App? App { get; set; }

    private AccountApp() { }

    internal static AccountApp Create(Guid accountId, Guid appId)
    {
        return new AccountApp
        {
            AccountId = accountId,
            AppId = appId,
        };
    }
}
