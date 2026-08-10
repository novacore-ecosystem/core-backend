using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Repositories;

public sealed class RefreshTokenRepo(AuthDbContext dbContext)
    : AuthBaseRepository<RefreshToken>(dbContext), IRefreshTokenRepository
{
}
