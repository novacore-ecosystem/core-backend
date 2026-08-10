namespace NovaCore.Shipping.Application.Abstractions.Persistence.TransportationPeople;

public interface ITransportationPersonWriteService
{

    Task<TransportationPerson> CreateAsync(
        Guid providerId,
        string fullName,
        PhoneNumber phoneNumber,
        Email? email = null,
        string? licenseNumber = null,
        Guid? userId = null,
        DateTime? joinedAt = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
