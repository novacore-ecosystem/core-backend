using System.Text.Json;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantConfig;

public sealed record UpdateTenantConfigCommand(
    Guid TenantId,
    LanguageCode? Language,
    JsonElement Config) : ICommand;
