using NovaCore.YarpApiGateway.Configuration;

namespace NovaCore.YarpApiGateway.Middleware;

public sealed class AuthorizationMiddleware(RequestDelegate next, GatewayOptions options)
{
    private readonly RequestDelegate _next = next;
    private readonly GatewayOptions _options = options;

    public async Task InvokeAsync(HttpContext context, IWebHostEnvironment webHost)
    {
        var path = context.Request.Path.Value ?? "";

        var serviceConfig = _options.Services.Values.FirstOrDefault(s => path.StartsWith(s.Path));
        var canReadSwagger = webHost.IsDevelopment()
            && path.EndsWith("/swagger.json");
        if (!canReadSwagger
            && serviceConfig?.RequireAuth == true
            && !context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                Success = false,
                Message = "Unauthorized access",
                MessageCode = "304"
            });
            return;
        }

        await _next(context);
    }
}

public static class AuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseGatewayAuthorization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthorizationMiddleware>();
    }
}
