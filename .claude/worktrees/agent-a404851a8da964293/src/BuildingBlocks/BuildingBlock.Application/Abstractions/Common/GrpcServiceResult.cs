namespace NovaCore.BuildingBlock.Application.Abstractions.Common;

/// <summary>
/// Generic result wrapper for gRPC service calls across microservices.
/// Used when calling external services via gRPC to standardize success/failure responses.
/// </summary>
/// <typeparam name="TData">Type of data returned on success</typeparam>
public sealed record GrpcServiceResult<TData>(
    bool Success,
    TData? Data = null,
    string? ErrorCode = null,
    string? Message = null)
    where TData : class;
