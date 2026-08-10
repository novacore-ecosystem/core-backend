using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.Deliveries.Repositories;

public sealed class DeliveryRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<Delivery, Guid>(dbContext), IDeliveryRepository
{
    public async Task<Delivery?> GetByTransportationIdAsync(Guid transportationId, CancellationToken ct = default)
        => await GetAsync(x => x.TransportationId, transportationId, ct);
}
