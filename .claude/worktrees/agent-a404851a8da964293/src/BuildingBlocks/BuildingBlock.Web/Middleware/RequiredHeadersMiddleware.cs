using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using Microsoft.AspNetCore.Http;

namespace NovaCore.BuildingBlock.Web.Middleware;

public sealed class RequiredHeadersMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        var metadata = endpoint?
            .Metadata
            .GetMetadata<HeadersMetadata>();

        if (metadata is not null)
        {
            foreach (var header in metadata.Headers)
            {
                var value = context.Request.Headers[header.Name].FirstOrDefault();

                if (header.Required && string.IsNullOrWhiteSpace(value))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    await context.Response.WriteAsJsonAsync(new
                    {
                        Success = false,
                        Message = $"{header.Name} header is required.",
                        MessageCode = "102"
                    });

                    return;
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    context.Items[header.Name] = value;
                }
            }
        }

        await _next(context);
    }
}
