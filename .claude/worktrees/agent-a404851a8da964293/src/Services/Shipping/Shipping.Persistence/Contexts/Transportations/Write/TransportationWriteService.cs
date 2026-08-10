using NovaCore.Shipping.Application.Abstractions.Persistence.Transportations;
using NovaCore.Shipping.Persistence.Contexts.Transportations.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Transportations.Write;

public sealed class TransportationWriteService(ITransportationRepository repo) : ITransportationWriteService
{
    public async Task<Transportation> CreateAsync(
        Guid shipmentId,
        Guid providerId,
        int attemptNo,
        Guid? costRuleId = null,
        DateTime? scheduledPickupAt = null,
        decimal? distanceKm = null,
        string? note = null,
        string? idempotencyKey = null,
        CancellationToken ct = default)
    {
        var transportation = Transportation.Create(
            shipmentId,
            providerId,
            attemptNo,
            costRuleId,
            scheduledPickupAt,
            distanceKm,
            note,
            idempotencyKey);

        await repo.AddAsync(transportation, ct);

        return transportation;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
