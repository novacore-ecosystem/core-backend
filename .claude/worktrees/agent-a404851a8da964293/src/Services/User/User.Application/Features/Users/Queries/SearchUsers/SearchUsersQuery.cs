using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.User.Application.Features.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(CriteriaRequest Criteria) : IQuery<PaginatedResult<SearchUsersItemResponse>>;

public sealed record SearchUsersItemResponse(
    Guid Id,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string DisplayName,
    UserStatus Status,
    string[] Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt);
