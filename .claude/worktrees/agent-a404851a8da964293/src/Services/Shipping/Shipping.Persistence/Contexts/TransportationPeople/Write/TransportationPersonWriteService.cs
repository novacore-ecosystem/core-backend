using NovaCore.Shipping.Application.Abstractions.Persistence.TransportationPeople;
using NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Write;

public sealed class TransportationPersonWriteService(ITransportationPersonRepository repo) : ITransportationPersonWriteService
{
    public async Task<TransportationPerson> CreateAsync(
        Guid providerId,
        string fullName,
        PhoneNumber phoneNumber,
        Email? email = null,
        string? licenseNumber = null,
        Guid? userId = null,
        DateTime? joinedAt = null,
        CancellationToken ct = default)
    {
        var person = TransportationPerson.Create(providerId, fullName, phoneNumber, email, licenseNumber, userId, joinedAt);

        await repo.AddAsync(person, ct);

        return person;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
