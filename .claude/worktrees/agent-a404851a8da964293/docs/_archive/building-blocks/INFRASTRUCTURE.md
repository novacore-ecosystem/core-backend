# BuildingBlock.Infrastructure - Common Middlewares

Shared ASP.NET Core middlewares available to every service without extra setup. For exception handling (also part of `BuildingBlock.Infrastructure`), see [EXCEPTIONS.md](EXCEPTIONS.md).

## Available Middlewares

| Middleware | Purpose | Usage |
|-----------|---------|-------|
| `LoggingMiddleware` | Log all HTTP requests | `app.UseLoggingMiddleware()` |
| `PerformanceMiddleware` | Track slow requests (>1s) | `app.UsePerformanceMiddleware()` |
| `RequestCorrelationMiddleware` | Track requests across services via `X-Correlation-Id` header | `app.UseRequestCorrelationMiddleware()` |

## Usage

**Individually:**
```csharp
app.UseRequestCorrelationMiddleware();
app.UseLoggingMiddleware();
app.UsePerformanceMiddleware();
```

**All at once:**
```csharp
app.UseCommonMiddlewares();
// Equivalent to:
// app.UseRequestCorrelationMiddleware();
// app.UseGlobalExceptionHandler();
// app.UseLoggingMiddleware();
// app.UsePerformanceMiddleware();
```

### Order matters

1. `RequestCorrelationMiddleware` — set correlation ID first
2. `GlobalExceptionHandler` — handle exceptions early
3. `LoggingMiddleware` — log requests
4. `PerformanceMiddleware` — monitor performance

## Full Program.cs Example

```csharp
using BuildingBlock.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGlobalExceptionHandler<YourService.Infrastructure.ExceptionHandling.GlobalExceptionHandler>();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddPresentation(builder.Configuration);

var app = builder.Build();

app.UseCommonMiddlewares();

app.UseSwagger();
app.MapCarter();
app.MapHealthChecks("/health");

app.Run();
```
