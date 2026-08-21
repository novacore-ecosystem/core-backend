using Mapster;

using NovaCore.Chat.Domain.ValueObjects;

namespace NovaCore.Chat.Application.Common.Mapping;

/// <summary>
/// Shared Value-Object-to-string conversion rules, reused by every Chat.Application feature that
/// maps a Domain entity carrying one of these VOs to a response DTO. Auto-discovered by Mapster's
/// assembly scan (TypeAdapterConfig.GlobalSettings.Scan in DependencyInjection.cs).
/// </summary>
public sealed class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EntityCode, string>().MapWith(src => src.Value);
    }
}
