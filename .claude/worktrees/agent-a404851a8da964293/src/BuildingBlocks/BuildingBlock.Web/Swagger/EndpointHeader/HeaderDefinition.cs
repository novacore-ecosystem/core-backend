namespace NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

public sealed record HeaderDefinition(
    string Name,
    bool Required = false,
    string? Description = null,
    string? DefaultValue = null);

