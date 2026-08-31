using System.Text.Json;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;

public sealed record UpdateTenantDictionaryCommand(
    Guid TenantId,
    LanguageCode? Language,
    JsonElement Dictionary) : ICommand;
