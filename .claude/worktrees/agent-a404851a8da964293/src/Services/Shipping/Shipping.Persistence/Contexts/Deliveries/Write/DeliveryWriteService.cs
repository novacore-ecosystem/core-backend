using NovaCore.Shipping.Application.Abstractions.Persistence.Deliveries;
using NovaCore.Shipping.Persistence.Contexts.Deliveries.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Deliveries.Write;

public sealed class DeliveryWriteService(IDeliveryRepository repo) : IDeliveryWriteService
{
    public async Task<Delivery> CreateAsync(
        Guid transportationId,
        string receiverName,
        PhoneNumber receiverPhone,
        ShippingAddress address,
        Money codAmount,
        CancellationToken ct = default)
    {
        var delivery = Delivery.Create(transportationId, receiverName, receiverPhone, address, codAmount);

        await repo.AddAsync(delivery, ct);

        return delivery;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
