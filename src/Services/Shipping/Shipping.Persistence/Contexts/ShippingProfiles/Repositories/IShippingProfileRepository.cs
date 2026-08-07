using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Repositories;

public interface IShippingProfileRepository : IRepository<ShippingProfile, Guid>
{
    Task<IReadOnlyList<ShippingProfile>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
