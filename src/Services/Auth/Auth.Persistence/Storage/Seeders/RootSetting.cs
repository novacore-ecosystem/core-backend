using NovaCore.BuildingBlock.Domain.Seeders;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Configuration-bound Root account id. Defaults to the historical
/// <see cref="SeedAuthData.Accounts.RootId"/> constant so existing dev/Docker environments keep
/// working unchanged; set the "Root:RootId" configuration key (Vault path
/// kv/nova-core/dev/services/auth for staging/production) to override it without recompiling.
/// </summary>
public sealed class RootSetting
{
    public const string Section = "Root";

    public Guid RootId { get; init; } = SeedAuthData.Accounts.RootId;
}
