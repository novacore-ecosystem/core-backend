namespace NovaCore.BuildingBlock.Infrastructure.Idempotency;

/// <summary>Endpoint metadata marker: presence means the endpoint requires an Idempotency-Key and is guarded by <see cref="IdempotencyMiddleware"/>.</summary>
public sealed class IdempotencyMetadata;
