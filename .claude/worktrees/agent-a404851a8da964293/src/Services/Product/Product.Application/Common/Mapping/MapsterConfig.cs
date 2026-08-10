using Mapster;

using NovaCore.Product.Domain.ValueObjects;

namespace NovaCore.Product.Application.Common.Mapping;

/// <summary>
/// Shared Value-Object-to-string conversion rules, reused by every NovaCore.Product.Application feature
/// that maps a Domain entity carrying one of these VOs to a response DTO. Auto-discovered by
/// Mapster's assembly scan (TypeAdapterConfig.GlobalSettings.Scan in DependencyInjection.cs) -
/// no manual registration needed at each call site.
/// </summary>
public sealed class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Sku, string>().MapWith(src => src.Value);
        config.NewConfig<Barcode, string>().MapWith(src => src.Value);
        config.NewConfig<ProductCode, string>().MapWith(src => src.Value);
        config.NewConfig<CategoryCode, string>().MapWith(src => src.Value);
        config.NewConfig<TagCode, string>().MapWith(src => src.Value);
        config.NewConfig<Slug, string>().MapWith(src => src.Value);
    }
}
