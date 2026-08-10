using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Repository;

using NovaCore.User.Application.Abstractions.Persistence.Users;
using NovaCore.User.Application.Features.Users.DTOs;

namespace NovaCore.User.Persistence.Contexts.Users.Write;

public sealed class UserWriteService(
    IRepository<UserEntity, Guid> repo,
    IUnitOfWork unitOfWork) : IUserWriteService
{
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
