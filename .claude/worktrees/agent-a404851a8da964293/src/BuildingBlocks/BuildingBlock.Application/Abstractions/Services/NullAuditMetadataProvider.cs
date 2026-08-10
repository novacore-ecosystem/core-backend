using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

/// <summary>Default no-op provider - registered so audit tracking never fails to resolve DI for services that don't configure a real one.</summary>
public sealed class NullAuditMetadataProvider : IAuditMetadataProvider
{
    public AuditMetadata Capture() => new();
}
