using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Repositories;

public interface IVerifiedShippingAddressRepository : IRepository<VerifiedShippingAddress, Guid>
{
    Task<IReadOnlyList<VerifiedShippingAddress>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
