using NovaCore.BuildingBlock.Infrastructure.Observability;
using NovaCore.BuildingBlock.Observability.Logging;
using NovaCore.BuildingBlock.Observability.Tracing;

using Serilog;

using NovaCore.YarpApiGateway;
using NovaCore.YarpApiGateway.Middleware;
using NovaCore.YarpApiGateway.Services;
var builder = WebApplication.CreateBuilder(args);

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
