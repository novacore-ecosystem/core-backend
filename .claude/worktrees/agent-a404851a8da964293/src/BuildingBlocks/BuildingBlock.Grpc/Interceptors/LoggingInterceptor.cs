using Grpc.Core;
using Grpc.Core.Interceptors;

using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Grpc.Interceptors;

/// <summary>
/// Server-side interceptor for logging gRPC calls.
/// Logs all incoming requests, responses, and errors.
/// </summary>
public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var startTime = DateTime.UtcNow;
        var method = context.Method;
        var peer = context.Peer;

        _logger.LogInformation(
            "gRPC Request: Method={Method}, Peer={Peer}",
            method,
            peer);

        try
        {
            var response = await continuation(request, context);
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "gRPC Request Completed: Method={Method}, Duration={DurationMs}ms, Status={Status}",
                method,
                duration.TotalMilliseconds,
                context.Status.StatusCode);

            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            _logger.LogError(
                ex,
                "gRPC Request Failed: Method={Method}, Duration={DurationMs}ms, Exception={Exception}",
                method,
                duration.TotalMilliseconds,
                ex.Message);

            throw;
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        var startTime = DateTime.UtcNow;
        var method = context.Method;

        _logger.LogInformation(
            "gRPC ServerStreaming Request: Method={Method}, Peer={Peer}",
            method,
            context.Peer);

        try
        {
            await continuation(request, responseStream, context);
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "gRPC ServerStreaming Completed: Method={Method}, Duration={DurationMs}ms",
                method,
                duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            _logger.LogError(
                ex,
                "gRPC ServerStreaming Failed: Method={Method}, Duration={DurationMs}ms",
                method,
                duration.TotalMilliseconds);

            throw;
        }
    }
}
