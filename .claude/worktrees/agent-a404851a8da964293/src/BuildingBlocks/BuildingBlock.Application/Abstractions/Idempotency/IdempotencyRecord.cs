namespace NovaCore.BuildingBlock.Application.Abstractions.Idempotency;

/// <summary>A previously-completed execution's response, cached so a duplicate request can replay it verbatim.</summary>
public sealed record IdempotencyRecord(
    int StatusCode,
    string? ContentType,
    byte[] Body,
    DateTime CreatedAtUtc);
