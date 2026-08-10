using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

/// <summary>
/// Extension point for context (actor, request, trace, ...) attached to an audit graph. Lives in
/// Application - not Persistence.Ef - so the change-tracking pipeline that consumes it never
/// couples audit metadata to EF, and any future non-EF provider (Dapper, Mongo) can reuse the
/// same abstraction. The default <see cref="NullAuditMetadataProvider"/> is registered by
/// NovaCore.BuildingBlock.Persistence.Ef so audit tracking works even for services (background workers,
/// jobs) with no HTTP context to capture.
/// </summary>
public interface IAuditMetadataProvider
{
    AuditMetadata Capture();
}
