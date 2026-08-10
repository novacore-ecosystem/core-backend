using System.Text.Json;

using NovaCore.Audit.Application.Features.AuditLogs.Commands.RecordAuditLog;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Audit;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using MediatR;

namespace NovaCore.Audit.Infrastructure.Messaging.Consumers;

/// <summary>
/// The single audit consumer in the project - every other service's AuditInterceptor publishes
/// exactly this one event type, so this is the only consumer Audit Service needs. Deserializes
/// and records - no business logic here, same "adapter only" rule as every other
/// IIntegrationEventConsumer in this codebase.
/// </summary>
public sealed class AuditIntegrationEventConsumer(
    ISender sender,
    IAppLogger<AuditIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(AuditIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<AuditIntegrationEvent>(message);
        if (integrationEvent == null)
        {
            logger.Warning("Failed to deserialize AuditIntegrationEvent");
            return;
        }

        var metadata = integrationEvent.Metadata;
        if (headers.TryGetValue("actor-id", out var headerActorId) && headerActorId != metadata?.Actor)
            metadata = metadata is null ? new AuditMetadata { Actor = headerActorId } : metadata with { Actor = headerActorId };

        await sender.Send(new RecordAuditLogCommand(
            integrationEvent.RootEntityType,
            integrationEvent.RootEntityId,
            integrationEvent.Metadata?.Service ?? string.Empty,
            integrationEvent.CorrelationId,
            integrationEvent.Root,
            metadata,
            integrationEvent.PublishedAt), ct);
    }
}
