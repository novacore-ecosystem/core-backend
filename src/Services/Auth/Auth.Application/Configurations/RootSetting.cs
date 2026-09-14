using NovaCore.BuildingBlock.Domain.Seeders;

namespace NovaCore.Auth.Application.Configurations;

/// <summary>
/// The configuration-driven identity and account information for the platform's singleton Root
/// account (see RootAccountSeeder). RootId is the stable Root <em>identity</em> - defaults to the
/// historical <see cref="SeedAuthData.Accounts.RootId"/> constant so existing dev/Docker
/// environments keep working unchanged. RootUsername/RootEmail/RootPassword are Root's
/// <em>account information</em> - mutable, reconciled idempotently against whatever account
/// currently holds RootId, and never used to determine which account is Root. Set under the
/// "Root:*" configuration keys (Vault path kv/nova-core/dev/services/auth for staging/production).
/// </summary>
/// <remarks>
/// Lives in Application (not Persistence, where it originated) because Application-layer code -
/// e.g. RefreshTokenHandler's Root App-membership bypass - needs to read it directly, and
/// Application must never depend on Persistence. Auth.Persistence still binds and consumes it
/// (its seeders resolve <c>IOptions&lt;RootSetting&gt;</c>) since Persistence depends on
/// Application, not the other way around.
/// </remarks>
public sealed class RootSetting
{
    public const string Section = "Root";

    public Guid Id { get; init; } = SeedAuthData.Accounts.RootId;

    /// <summary>
    /// Required - unlike RootId, no fabricated default. A missing value fails startup
    /// validation (see RootSettingValidator) rather than silently provisioning a broken Root account
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Required - unlike RootId, no fabricated default. A missing value fails startup
    /// validation (see RootSettingValidator) rather than silently provisioning a broken Root account
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Required - unlike RootId, no fabricated default. Only ever used as input to the identity system's
    /// own password hashing/verification APIs (UserManager) - never persisted or compared as plaintext
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
