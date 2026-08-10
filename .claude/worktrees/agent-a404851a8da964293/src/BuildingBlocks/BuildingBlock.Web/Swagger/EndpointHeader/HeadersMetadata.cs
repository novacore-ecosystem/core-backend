namespace NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

public sealed class HeadersMetadata(IEnumerable<HeaderDefinition> headers)
{
    public IReadOnlyList<HeaderDefinition> Headers { get; } = [.. headers];
}
