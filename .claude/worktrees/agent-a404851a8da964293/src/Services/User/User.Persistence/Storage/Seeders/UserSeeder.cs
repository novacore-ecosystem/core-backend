using NovaCore.BuildingBlock.Domain.Seeders;

using NovaCore.User.Persistence.Engine;

namespace NovaCore.User.Persistence.Storage.Seeders;

public sealed class UserSeeder(UserDbContext context)
{
    public async Task SeedAsync()
    {
        if (await context.Users.AnyAsync())
            return;

        var users = SeedAuthData.Accounts.Default
            .Select(account =>
            {
                // Shares the Account's id, same correlation rule OnUserInitiated uses for
                // self-registered accounts - see User.Create's id override.
                var user = UserEntity.Create(account.Username, account.Username, UserType.Administrator, id: account.Id);
                user.UpdateProfile(PersonalName.Create(account.Username, null, account.Username));
                user.AddContact(ContactType.Email, account.Email, isPrimary: true);
                user.AddContact(ContactType.Phone, "1234567890", isPrimary: true);
                return user;
            })
            .ToArray();

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}
