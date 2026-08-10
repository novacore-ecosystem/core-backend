using Grpc.Core;
using Grpc.Core.Interceptors;

using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Grpc.Interceptors;

/// <summary>
/// Server-side interceptor for consistent error handling in gRPC.
/// Converts exceptions to appropriate gRPC status codes and logs them.
/// </summary>
public sealed class ErrorHandlingInterceptor(ILogger<ErrorHandlingInterceptor> logger) : Interceptor
{
    private readonly ILogger<ErrorHandlingInterceptor> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException rpcEx)
        {
            _logger.LogWarning(
                rpcEx,
                "gRPC RpcException: Method={Method}, Status={Status}",
                context.Method,
                rpcEx.Status.StatusCode);
            throw;
        }
        catch (ArgumentNullException argEx)
        {
            _logger.LogError(
                argEx,
                "gRPC Validation Error: Method={Method}, Argument={Argument}",
                context.Method,
                argEx.ParamName);

            throw new RpcException(
                new Status(StatusCode.InvalidArgument, argEx.Message, argEx));
        }
        catch (InvalidOperationException invEx)
        {
            _logger.LogWarning(
                invEx,
                "gRPC Invalid Operation: Method={Method}",
                context.Method);

            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, invEx.Message, invEx));
        }
        catch (UnauthorizedAccessException unAuthEx)
        {
            _logger.LogWarning(
                unAuthEx,
                "gRPC Unauthorized: Method={Method}, Peer={Peer}",
                context.Method,
                context.Peer);

            throw new RpcException(
                new Status(StatusCode.Unauthenticated, "Unauthorized", unAuthEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "gRPC Unhandled Exception: Method={Method}",
                context.Method);

            throw new RpcException(
                new Status(StatusCode.Internal, "An internal error occurred", ex));
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (RpcException rpcEx)
        {
            _logger.LogWarning(
                rpcEx,
                "gRPC ServerStreaming RpcException: Method={Method}",
                context.Method);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "gRPC ServerStreaming Exception: Method={Method}",
                context.Method);

            throw new RpcException(
                new Status(StatusCode.Internal, "An internal error occurred", ex));
        }
    }
}
