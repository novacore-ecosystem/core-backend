using NovaCore.BuildingBlock.SharedKernel.Text;

namespace NovaCore.User.Application.Features.Users.DTOs;

/// <summary>
/// Flat read-model projection of the User aggregate (Username/DisplayName + Profile.PersonalName
/// + the primary Email/Phone UserContact) - Application never sees the aggregate itself, only
/// this shape. Reused verbatim across the User Detail cache, gRPC lookups, and Search projection,
/// so a future field addition touches one type instead of four parallel DTOs.
/// </summary>
public sealed record UserReadModel(
    Guid Id,
    Guid TenantId,
    string UserName,
    string DisplayName,
    string Email,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    UserStatus Status,
    string[] Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    // Search-index parity fields (see UserCriteriaDefinition/UserSearchProjectionBuilder) -
    // derived from PhoneNumber rather than persisted, since UserContact stores only the raw value.
    public string PhoneSearch => PhoneNormalizer.Normalize(PhoneNumber);
    public string PhoneReverse => PhoneNormalizer.Reverse(PhoneSearch);
}
