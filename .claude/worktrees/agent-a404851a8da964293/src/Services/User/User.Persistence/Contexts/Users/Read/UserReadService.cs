using NovaCore.User.Application.Abstractions.Persistence.Users;
using NovaCore.User.Application.Features.Users.DTOs;
using NovaCore.User.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.User.Persistence.Contexts.Users.Read;

public sealed class UserReadService(UserDbContext dbContext) : IUserReadService
{
    public async Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(ToReadModel)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<UserReadModel>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        return await dbContext.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(ToReadModel)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserReadModel>> GetAllAsync(int skip, int take, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .Select(ToReadModel)
            .ToListAsync(ct);
    }

    // Email/PhoneNumber resolve to the primary contact of that type (falling back to any contact
    // of that type if none is marked primary) - the aggregate no longer stores them as columns.
    private static readonly System.Linq.Expressions.Expression<Func<UserEntity, UserReadModel>> ToReadModel = u => new UserReadModel(
        u.Id,
        u.TenantId,
        u.Username,
        u.DisplayName,
        u.Contacts
            .Where(c => c.ContactType == ContactType.Email)
            .OrderByDescending(c => c.IsPrimary)
            .Select(c => c.Value)
            .FirstOrDefault() ?? string.Empty,
        u.Contacts
            .Where(c => c.ContactType == ContactType.Phone)
            .OrderByDescending(c => c.IsPrimary)
            .Select(c => c.Value)
            .FirstOrDefault() ?? string.Empty,
        u.Profile != null ? u.Profile.PersonalName.FirstName : string.Empty,
        u.Profile != null ? (u.Profile.PersonalName.MiddleName ?? string.Empty) : string.Empty,
        u.Profile != null ? u.Profile.PersonalName.LastName : string.Empty,
        u.Status,
        u.RoleAssignments
            // Inlined UserRoleAssignment.IsEffective - a C# computed property can't be translated
            // into SQL by EF Core, so the predicate is duplicated here rather than reused.
            .Where(ra => ra.Status == UserRoleAssignmentStatus.Active && (ra.ExpiredAt == null || ra.ExpiredAt > DateTime.UtcNow))
            .Select(ra => ra.Role.Key.Value)
            .ToArray(),
        u.CreatedAt,
        u.UpdatedAt);
}
