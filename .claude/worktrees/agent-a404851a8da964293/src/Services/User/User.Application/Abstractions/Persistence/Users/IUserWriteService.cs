using NovaCore.User.Application.Features.Users.DTOs;

namespace NovaCore.User.Application.Abstractions.Persistence.Users;

public interface IUserWriteService
{
    /// <summary>Commits via ExecuteTransactionAsync (ConflictException on unique-index races). Returns the created User - CreateUserHandler needs it to build UserProfileCreatedIntegrationEvent.</summary>
    Task<UserReadModel> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

    /// <summary>Mirrors an Account created in Auth, commits via bare SaveChangesAsync. Returns the created User - OnUserInitiatedHandler needs its Id for OnUserSearchSyncRequiredEvent.</summary>
    Task<UserReadModel> SyncFromAccountInitiationAsync(SyncUserRequest request, CancellationToken ct = default);

    Task UpdateProfileDetailsAsync(Guid id, string firstName, string middleName, string lastName, string phoneNumber, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<int> DeleteWithNoTrackingAsync(Guid id, CancellationToken ct = default);
}
