using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.YarpApiGateway.Middleware;

/// <summary>
/// The Gateway's only tracing responsibility: forward an incoming X-Correlation-Id unchanged, or
/// generate one when absent so every downstream service sees the same id for this request. No
/// other request-tracing/business logic belongs here - see docs/services/gateway.md.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var hasCorrelationId = context.Request.Headers.TryGetValue(HeaderKeyConstant.CorrelationId, out var value)
            && !string.IsNullOrWhiteSpace(value);

        if (!hasCorrelationId)
            context.Request.Headers[HeaderKeyConstant.CorrelationId] = Guid.NewGuid().ToString();

        await next(context);
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
