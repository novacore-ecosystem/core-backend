using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Audit;
using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NovaCore.BuildingBlock.Persistence.Ef.Interceptors;

/// <summary>
/// Collects tracked changes into per-Aggregate-Root <see cref="AuditIntegrationEvent"/>s (exactly
/// one per changed root instance - never one per entity), enqueued onto the same DbContext's
/// Outbox table so they commit atomically with the business change they describe.
///
/// Two gates decide whether an entity is even considered: it must implement <see cref="IAuditable"/>
/// (a cheap reference-type check, tried first) AND be registered via <c>ConfigureAuditHierarchy</c>
/// (a dictionary lookup) - both are required so a type can never end up half-configured (marked
/// but not placed in the graph, or placed without declaring intent). Properties carrying
/// [AuditIgnore] never reach the change snapshot. All the actual grouping/tree-building logic is
/// provider-agnostic and lives in <see cref="AuditGraphBuilder"/> (NovaCore.BuildingBlock.Persistence) -
/// this class's only job is extracting <see cref="AuditTrackedEntity"/> snapshots from EF's
/// ChangeTracker (primary keys via EF's own metadata, not reflection-guessing) and handing them
/// off to that shared algorithm.
/// </summary>
public sealed class AuditInterceptor(
    IAuditHierarchyRegistry hierarchyRegistry,
    IAuditMetadataProvider metadataProvider) : ISaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<Type, HashSet<string>> IgnoredPropertiesByType = new();

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        CollectAndEnqueueAuditEvents(eventData.Context, hierarchyRegistry, metadataProvider);

        return ValueTask.FromResult(result);
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CollectAndEnqueueAuditEvents(eventData.Context, hierarchyRegistry, metadataProvider);

        return result;
    }

    private static void CollectAndEnqueueAuditEvents(
        Microsoft.EntityFrameworkCore.DbContext? context,
        IAuditHierarchyRegistry registry,
        IAuditMetadataProvider metadataProvider)
    {
        if (context is not IOutboxDbContext outboxContext)
            return;

        // Materialized eagerly: enqueueing Outbox rows below mutates the same ChangeTracker an
        // in-progress Entries() enumeration would be invalidated by.
        var trackedEntities = context.ChangeTracker.Entries()
            .Where(entry => entry.Entity is IAuditable)
            .Select(entry => ToTrackedEntity(entry, registry))
            .Where(entity => entity is not null)
            .Select(entity => entity!)
            .ToList();

        var graphs = AuditGraphBuilder.Build(trackedEntities, registry);
        if (graphs.Count == 0)
            return;

        // Captured once, reused for every root graph in this SaveChanges batch.
        var metadata = metadataProvider.Capture();

        foreach (var graph in graphs)
        {
            var auditEvent = new AuditIntegrationEvent(graph.RootEntityType, graph.RootEntityId, graph.Root, metadata);

            var payload = JsonSerializer.Serialize(auditEvent);
            var message = OutboxMessage.Create(
                auditEvent.EventType,
                auditEvent.EventType.ToLowerInvariant(),
                payload,
                auditEvent.CorrelationId,
                metadata.Actor,
                metadata.Actor is not null ? "user" : "system");

            outboxContext.OutboxMessages.Add(message);
        }
    }

    private static AuditTrackedEntity? ToTrackedEntity(EntityEntry entry, IAuditHierarchyRegistry registry)
    {
        if (!registry.TryGetMetadata(entry.Entity.GetType(), out var meta))
            return null;

        var isChangedState = entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;

        // An Unchanged entry only matters as structural scaffolding for a changed descendant's
        // ancestor path - still emitted (HasChanges=false, no property comparison performed), so
        // AuditGraphBuilder can resolve that path without losing the hierarchy in between.
        var changes = isChangedState ? BuildChanges(entry) : [];
        var hasChanges = isChangedState && changes.Count > 0;

        var action = entry.State switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Deleted => AuditAction.Deleted,
            _ => AuditAction.Updated,
        };

        var parentId = meta.ParentType is null
            ? null
            : Format(meta.ParentIdAccessor?.Invoke(entry.Entity));

        return new AuditTrackedEntity(entry.Entity.GetType(), GetOwnId(entry), parentId, hasChanges, action, changes);
    }

    private static List<AuditFieldChange> BuildChanges(EntityEntry entry)
    {
        var ignoredProperties = GetIgnoredProperties(entry.Entity.GetType());
        var changes = new List<AuditFieldChange>();

        foreach (var property in entry.Properties)
        {
            if (ignoredProperties.Contains(property.Metadata.Name))
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    changes.Add(new AuditFieldChange(property.Metadata.Name, null, Format(property.CurrentValue)));
                    break;

                case EntityState.Deleted:
                    changes.Add(new AuditFieldChange(property.Metadata.Name, Format(property.OriginalValue), null));
                    break;

                case EntityState.Modified:
                    if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                        changes.Add(new AuditFieldChange(
                            property.Metadata.Name, Format(property.OriginalValue), Format(property.CurrentValue)));
                    break;
            }
        }

        return changes;
    }

    private static HashSet<string> GetIgnoredProperties(Type entityType)
    {
        return IgnoredPropertiesByType.GetOrAdd(entityType, type =>
            [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.IsDefined(typeof(AuditIgnoreAttribute), inherit: true))
                .Select(p => p.Name)]);
    }

    private static string GetOwnId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
            return string.Empty;

        var values = key.Properties.Select(p => Format(entry.Property(p.Name).CurrentValue));
        return string.Join(",", values);
    }

    private static string? Format(object? value) => value?.ToString();
}
