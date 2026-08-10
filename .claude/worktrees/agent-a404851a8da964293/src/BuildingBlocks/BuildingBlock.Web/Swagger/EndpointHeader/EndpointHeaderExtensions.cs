using Microsoft.AspNetCore.Builder;

namespace NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

public static class EndpointHeaderExtensions
{
    public static TBuilder Headers<TBuilder>(
        this TBuilder builder,
        params HeaderDefinition[] headers)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new HeadersMetadata(headers));

        return builder;
    }
}
