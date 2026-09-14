using NovaCore.Auth.Application.Abstractions.Persistence.RefreshTokens;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Contexts.RefreshTokens.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Read;

public sealed class RefreshTokenReadService(IRefreshTokenRepository refreshTokenRepo)
    : IRefreshTokenReadService, IPersistenceService
{
    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await refreshTokenRepo.GetByUserIdAsync(userId, ct);
    }
}
