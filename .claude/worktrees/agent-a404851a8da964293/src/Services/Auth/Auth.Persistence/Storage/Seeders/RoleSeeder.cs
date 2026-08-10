using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Domain.Seeders;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

public class RoleSeeder(AuthDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = SeedAuthData.Roles.Default
            .Select(r => Role.Create(r.Name, RoleCode.Create(r.Name), r.Description, isSystemRole: true))
            .ToList();

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
