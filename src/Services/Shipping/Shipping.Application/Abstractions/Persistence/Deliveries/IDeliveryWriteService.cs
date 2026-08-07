namespace NovaCore.Shipping.Application.Abstractions.Persistence.Deliveries;

public interface IDeliveryWriteService
{

    Task<Delivery> CreateAsync(
        Guid transportationId,
        string receiverName,
        PhoneNumber receiverPhone,
        ShippingAddress address,
        Money codAmount,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
