using System.Diagnostics.CodeAnalysis;

namespace NovaCore.BuildingBlock.Persistence.Audit;

/// <summary>
/// Read-only lookup of resolved audit hierarchy metadata, built once by <c>ConfigureAuditHierarchy</c>
/// and registered as a singleton - metadata is never rebuilt per SaveChanges.
/// </summary>
public interface IAuditHierarchyRegistry
{
    bool TryGetMetadata(Type entityType, [MaybeNullWhen(false)] out AuditEntityMetadata metadata);
}
