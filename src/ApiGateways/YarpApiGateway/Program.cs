using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;
using NovaCore.BuildingBlock.Web.Extensions;

using Serilog;

using NovaCore.YarpApiGateway;
using NovaCore.YarpApiGateway.Middleware;
using NovaCore.YarpApiGateway.Services;
var builder = WebApplication.CreateBuilder(args);

await builder.Configuration.AddVaultSecretsAsync();

builder.Host.UseSerilog((context, config) => config.ConfigureAppLogging(context.Configuration, "yarp-api-gateway"));

builder.Services
    .AddGatewayServices(builder.Configuration)
    .AddHttpContextAccessor()
    .AddOpenTelemetryObservability(builder.Configuration, "yarp-api-gateway", tracing => tracing
        .AddInfrastructureTracing());

var app = builder.Build();

app.UseRedisTracing();
app.MapHealthChecks("/health");
app.UseCorrelationId();
// Must run before authentication/authorization: the CORS middleware answers preflight
// OPTIONS requests directly and short-circuits the pipeline, so a browser's preflight never
// reaches UseGatewayAuthorization (which would otherwise 401 it - preflight requests are
// sent without credentials/cookies per the CORS spec, so RequireAuth routes always look
// unauthenticated to it).
app.UseCors(DependencyInjection.CorsPolicyName);
app.UseAuthentication();
app.UseRefreshTokenFilter();
app.UseGatewayAuthorization();

var swaggerAggregator = app.Services.GetRequiredService<ISwaggerAggregator>();

app.MapGet("/swagger", swaggerAggregator.ServeSwaggerIndexAsync);

app.MapReverseProxy(pipeline =>
{
    pipeline.UseSessionAffinity();
    pipeline.UseLoadBalancing();
});

app.Run();
