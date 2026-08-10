using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace NovaCore.BuildingBlock.Web.Swagger.Filters;

public sealed class HeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<HeadersMetadata>()
            .FirstOrDefault();

        if (metadata is null)
            return;

        operation.Parameters ??= [];

        foreach (var header in metadata.Headers)
        {
            if (operation.Parameters.Any(p =>
                p.In == ParameterLocation.Header &&
                p.Name.Equals(header.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = header.Name,
                In = ParameterLocation.Header,
                Required = header.Required,
                Description = header.Description,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = header.DefaultValue is null
                        ? null
                        : new OpenApiString(header.DefaultValue)
                }
            });
        }
    }
}
