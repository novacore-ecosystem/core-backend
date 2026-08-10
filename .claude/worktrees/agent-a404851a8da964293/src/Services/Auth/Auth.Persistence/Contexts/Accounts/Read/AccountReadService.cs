using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Read;

public sealed class AccountReadService(AuthDbContext dbContext) : IAccountReadService
{
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(u => u.AccountRoles)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}
