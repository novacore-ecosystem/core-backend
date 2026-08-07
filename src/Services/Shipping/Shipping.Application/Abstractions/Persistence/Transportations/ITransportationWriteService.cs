namespace NovaCore.Shipping.Application.Abstractions.Persistence.Transportations;

public interface ITransportationWriteService
{

    Task<Transportation> CreateAsync(
        Guid shipmentId,
        Guid providerId,
        int attemptNo,
        Guid? costRuleId = null,
        DateTime? scheduledPickupAt = null,
        decimal? distanceKm = null,
        string? note = null,
        string? idempotencyKey = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
