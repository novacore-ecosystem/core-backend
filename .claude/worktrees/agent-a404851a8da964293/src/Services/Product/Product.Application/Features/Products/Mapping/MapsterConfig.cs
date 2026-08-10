using Mapster;

namespace NovaCore.Product.Application.Features.Products.Mapping;

public sealed class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig cfg)
    {
        MapProductConfig(cfg);
    }

    private static void MapProductConfig(TypeAdapterConfig cfg)
    {
    }
}
