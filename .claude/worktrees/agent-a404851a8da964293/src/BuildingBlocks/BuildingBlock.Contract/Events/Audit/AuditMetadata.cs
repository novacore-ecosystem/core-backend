namespace NovaCore.BuildingBlock.Contract.Events.Audit;

/// <summary>
/// Optional, extensible context captured alongside an audit graph. Every field is nullable and
/// additive by design - new context (e.g. a future field) is a new optional property here, never
/// a breaking change to <see cref="AuditIntegrationEvent"/> or the entities that produce it.
/// Populated by an <c>IAuditMetadataProvider</c> (NovaCore.BuildingBlock.Application) - deliberately not
/// coupled to EF or any other persistence provider.
/// </summary>
public sealed record AuditMetadata
{
    public string? Actor { get; init; }
    public string? Service { get; init; }
    public string? ClientIp { get; init; }
    public string? UserAgent { get; init; }
    public string? BusinessAction { get; init; }
    public string? Reason { get; init; }
    public string? RequestPath { get; init; }
    public string? TraceId { get; init; }
}
