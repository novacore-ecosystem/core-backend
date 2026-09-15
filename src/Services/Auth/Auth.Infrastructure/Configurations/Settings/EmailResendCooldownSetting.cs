using NovaCore.BuildingBlock.Infrastructure.Configurations;

namespace NovaCore.Auth.Infrastructure.Configurations.Settings;

/// <summary>The shared window <see cref="Services.AuthEmailRequestService"/> enforces between two email sends for the same (purpose, email) - the single source of truth Register's initial send and ResendEmail's explicit resend are both bound by.</summary>
public sealed class EmailResendCooldownSetting : ISetting
{
    public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(30);
}
