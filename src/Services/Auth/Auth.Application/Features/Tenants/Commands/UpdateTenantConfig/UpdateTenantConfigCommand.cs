using System.Text.Json;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;

/// <summary>Merge-updates one locale's ConfigurationJson - a null Language targets the tenant-wide
/// fallback/default configuration (see TenantLocale). The payload is merged onto the stored JSON
/// (unspecified keys preserved, see JsonMergeHelper), never a wholesale replace.</summary>
public sealed record UpdateTenantConfigCommand(
    Guid TenantId,
    string? Language,
    JsonElement Config) : ICommand;
