using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Domain.Enums;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Infrastructure.Idempotency;

/// <summary>
/// Enforces idempotency for endpoints marked with <see cref="IdempotencyMetadata"/> (via
/// <c>.RequireIdempotency()</c>): acquires a distributed lock for the operation+key, replays a
/// cached response for a duplicate request, otherwise runs the endpoint and caches the result
/// only on success. Endpoints without the metadata pass straight through untouched.
/// </summary>
public sealed class IdempotencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IDistributedLockProvider lockProvider,
        IIdempotencyStore idempotencyStore,
        IdempotencyOptions options,
        ILogger<IdempotencyMiddleware> logger)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IdempotencyMetadata>() is null)
        {
            await next(context);
            return;
        }

        var ct = context.RequestAborted;
        var idempotencyKey = context.Request.Headers[options.HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Fail(
                    $"{options.HeaderName} header is required for this operation.",
                    MessageCode.RequiredFieldMissing),
                ct);
            return;
        }

        var routePattern = (endpoint as RouteEndpoint)?.RoutePattern.RawText ?? context.Request.Path.Value ?? endpoint.DisplayName ?? "unknown";
        var operation = $"{context.Request.Method}:{routePattern}";
        var resource = $"{operation}:{idempotencyKey}";

        await using var @lock = await lockProvider.AcquireAsync(
            resource,
            TimeSpan.FromSeconds(options.LockExpirationSeconds),
            TimeSpan.FromSeconds(options.LockTimeoutSeconds),
            ct);

        if (@lock is null)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object?>.Fail(
                    "A request with the same Idempotency-Key is already being processed. Please retry shortly.",
                    MessageCode.Conflict),
                ct);
            return;
        }

        var cached = await idempotencyStore.GetAsync(resource, ct);
        if (cached is not null)
        {
            logger.LogInformation(
                "Duplicate request detected for operation {Operation} with key {IdempotencyKey}; returning cached response",
                operation, idempotencyKey);

            context.Response.StatusCode = cached.StatusCode;
            if (!string.IsNullOrEmpty(cached.ContentType))
                context.Response.ContentType = cached.ContentType;

            await context.Response.Body.WriteAsync(cached.Body, ct);
            return;
        }

        logger.LogInformation(
            "New execution started for operation {Operation} with key {IdempotencyKey}",
            operation, idempotencyKey);

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        buffer.Seek(0, SeekOrigin.Begin);

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            if (buffer.Length <= options.MaxCachedResponseSizeBytes)
            {
                var record = new IdempotencyRecord(
                    context.Response.StatusCode,
                    context.Response.ContentType,
                    buffer.ToArray(),
                    DateTime.UtcNow);

                await idempotencyStore.SaveAsync(resource, record, TimeSpan.FromMinutes(options.RecordExpirationMinutes), ct);

                logger.LogInformation(
                    "Idempotency record created for operation {Operation} with key {IdempotencyKey}, expires in {ExpirationMinutes}m",
                    operation, idempotencyKey, options.RecordExpirationMinutes);
            }
            else
            {
                logger.LogWarning(
                    "Response for operation {Operation} with key {IdempotencyKey} was {Size} bytes, exceeding MaxCachedResponseSizeBytes ({Max}); not caching",
                    operation, idempotencyKey, buffer.Length, options.MaxCachedResponseSizeBytes);
            }
        }
        else
        {
            logger.LogInformation(
                "Execution for operation {Operation} with key {IdempotencyKey} failed with status {StatusCode}; no record stored, safe to retry",
                operation, idempotencyKey, context.Response.StatusCode);
        }

        await buffer.CopyToAsync(originalBody, ct);
    }
}
