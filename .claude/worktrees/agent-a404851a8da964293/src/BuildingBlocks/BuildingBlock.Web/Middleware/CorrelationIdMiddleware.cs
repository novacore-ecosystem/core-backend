using System.Diagnostics;

using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Web.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var hasCorrelationId = context.Request.Headers.TryGetValue(HeaderKeyConstant.CorrelationId, out var value)
            && !string.IsNullOrWhiteSpace(value);
        var correlationId = hasCorrelationId ? value.ToString() : Guid.NewGuid().ToString();

        // Write the (possibly-generated) id back onto the request so it's available to any
        // outbound call this service makes further downstream, not just to this request's own logs.
        if (!hasCorrelationId)
            context.Request.Headers[HeaderKeyConstant.CorrelationId] = correlationId;

        Activity.Current?.SetTag("correlationId", correlationId);

        using (logger.BeginScope(new Dictionary<string, object> { ["correlationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}
