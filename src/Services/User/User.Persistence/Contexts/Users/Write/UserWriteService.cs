using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.User.Application.Abstractions.Persistence.Users;
using NovaCore.User.Application.Features.Users.DTOs;
using NovaCore.User.Persistence.Engine;

namespace NovaCore.User.Persistence.Contexts.Users.Write;

public sealed class UserWriteService(
    IRepository<UserEntity, Guid> repo,
    UserDbContext dbContext,
    IUnitOfWork unitOfWork) : IUserWriteService
{
    // Large enough to keep round trips low, small enough to keep each UPDATE/INSERT statement's
    // array parameter a reasonable size - not a hard Postgres limit (accountIds.Contains(...)
    // binds as one "= ANY(@p)" array parameter either way), just a conservative batch unit.
    private const int BatchSize = 500;

    public async Task<UserReadModel> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var user = UserEntity.Create(request.Username, request.DisplayName, request.UserType);
        user.UpdateProfile(PersonalName.Create(request.FirstName, request.MiddleName, request.LastName));
        user.AddContact(ContactType.Email, request.Email, isPrimary: true);
        user.AddContact(ContactType.Phone, request.PhoneNumber, isPrimary: true);

        await repo.AddAsync(user, ct);

        return ToReadModel(user);
    }

    public async Task<UserReadModel> SyncFromAccountInitiationAsync(SyncUserRequest request, CancellationToken ct = default)
    {
        // Auth already minted the Account's id - this User row is correlated by sharing it, not
        // by a separate foreign key. See User.Create's id override.
        var user = UserEntity.Create(request.Username, request.Username, UserType.Customer, id: request.AccountId);
        user.UpdateProfile(PersonalName.Create(request.FirstName, request.MiddleName, request.LastName));
        user.AddContact(ContactType.Email, request.Email, isPrimary: true);
        user.AddContact(ContactType.Phone, request.PhoneNumber, isPrimary: true);

        await repo.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ToReadModel(user);
    }

    public async Task UpdateProfileDetailsAsync(Guid id, string firstName, string middleName, string lastName, string phoneNumber, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, user =>
        {
            user.UpdateProfile(PersonalName.Create(firstName, middleName, lastName));

            var phoneContact = user.Contacts.FirstOrDefault(c => c.ContactType == ContactType.Phone && c.IsPrimary);
            if (phoneContact is not null)
                phoneContact.UpdateValue(phoneNumber);
            else
                user.AddContact(ContactType.Phone, phoneNumber, isPrimary: true);
        }, ct);
        // no commit here - UpdateUserHandler wraps this call in its own ExecuteTransactionAsync
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteByIdAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.DeleteWithNoTrackingAsync(u => u.Id == id, ct);
    }

    public async Task RebuildAuthorizationSnapshotsAsync(IReadOnlyList<AccountAuthorizationUpdate> updates, CancellationToken ct = default)
    {
        foreach (var batch in updates.Chunk(BatchSize))
        {
            var userIds = batch.Select(u => u.UserId).ToArray();
            var existingUserIds = await dbContext.UserAuthorizationSnapshots
                .Where(s => userIds.Contains(s.UserId))
                .Select(s => s.UserId)
                .ToHashSetAsync(ct);

            var normalized = batch
                .Select(u => new { u.UserId, Permissions = u.Permissions.OrderBy(p => p, StringComparer.Ordinal).ToArray() })
                .ToList();

            // Grouped by identical resulting permission set - accounts sharing a Role (the common
            // case) collapse into one ExecuteUpdateAsync instead of one statement per account. The
            // separator is arbitrary since permission keys can't contain it.
            var updateGroups = normalized
                .Where(u => existingUserIds.Contains(u.UserId))
                .GroupBy(u => string.Join('|', u.Permissions));

            var now = DateTime.UtcNow;
            foreach (var group in updateGroups)
            {
                var groupUserIds = group.Select(u => u.UserId).ToArray();
                var permissions = group.First().Permissions;

                await dbContext.UserAuthorizationSnapshots
                    .Where(s => groupUserIds.Contains(s.UserId))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Permissions, permissions)
                        .SetProperty(s => s.Version, s => s.Version + 1)
                        .SetProperty(s => s.UpdatedAt, now), ct);
            }

            // First-ever authorization change for these accounts - no snapshot row exists yet
            // (UserAuthorizationSnapshot is created lazily, not at User creation), so there is
            // nothing for ExecuteUpdateAsync to match. Inserted directly rather than loading/
            // tracking the owning User aggregate, keeping this path batch-sized too.
            var missing = normalized.Where(u => !existingUserIds.Contains(u.UserId)).ToList();
            if (missing.Count > 0)
            {
                var newSnapshots = missing.Select(u =>
                {
                    var snapshot = UserAuthorizationSnapshot.Create(u.UserId);
                    snapshot.Rebuild(u.Permissions);
                    return snapshot;
                });

                await dbContext.UserAuthorizationSnapshots.AddRangeAsync(newSnapshots, ct);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    // Freshly-created, not queried back - Roles is intentionally empty here (a just-added
    // UserRoleAssignment has no loaded Role navigation to read Key from), which is fine since
    // neither CreateUserHandler nor OnUserInitiatedHandler read Roles off the returned model.
    private static UserReadModel ToReadModel(UserEntity user) => new(
        user.Id,
        user.TenantId,
        user.Username,
        user.DisplayName,
        user.Contacts.FirstOrDefault(c => c.ContactType == ContactType.Email)?.Value ?? string.Empty,
        user.Contacts.FirstOrDefault(c => c.ContactType == ContactType.Phone)?.Value ?? string.Empty,
        user.Profile?.PersonalName.FirstName ?? string.Empty,
        user.Profile?.PersonalName.MiddleName ?? string.Empty,
        user.Profile?.PersonalName.LastName ?? string.Empty,
        user.Status,
        [],
        user.CreatedAt,
        user.UpdatedAt);
}
