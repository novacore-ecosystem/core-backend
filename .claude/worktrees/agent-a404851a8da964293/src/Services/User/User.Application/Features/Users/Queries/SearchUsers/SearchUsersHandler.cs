using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.SharedKernel.Text;

using NovaCore.User.Application.Abstractions.Search;
using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Queries.SearchUsers;

/// <summary>
/// Elasticsearch-backed - one query composing keyword + role + status + phone + pagination +
/// sort, never a mix of Elasticsearch + PostgreSQL. Replaces the previous Postgres/NovaCore.BuildingBlock.Criteria
/// execution path (IUserProfileReadService.SearchAsync); UserCriteriaDefinition/SearchUsersValidator
/// are kept as a pure request-shape whitelist (field/operator validation), independent of which
/// engine actually executes the query. See docs/reference/search.md and
/// docs/tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md.
/// </summary>
public sealed class SearchUsersHandler(
    IUserSearchRepository searchRepository,
    IUserDisplayNameFormatter displayNameFormatter,
    ICurrentLocaleService currentLocale) : IQueryHandler<SearchUsersQuery, PaginatedResult<SearchUsersItemResponse>>
{
    public async Task<PaginatedResult<SearchUsersItemResponse>> Handle(SearchUsersQuery request, CancellationToken ct = default)
    {
        var criteria = BuildCriteria(request.Criteria);
        var (documents, totalCount) = await searchRepository.SearchAsync(criteria, ct);
        var locale = currentLocale.GetLocale();

        var items = documents.Select(d => new SearchUsersItemResponse(
            d.UserId,
            d.Email,
            d.UserName,
            d.PhoneNumber,
            d.FirstName,
            d.MiddleName,
            d.LastName,
            displayNameFormatter.Format(d.FirstName, d.MiddleName, d.LastName, locale),
            Enum.Parse<UserStatus>(d.Status),
            [.. d.Roles],
            d.CreatedAt,
            d.UpdatedAt)).ToList();

        return PaginatedResult<SearchUsersItemResponse>.Create(items, request.Criteria.Page, request.Criteria.PageSize, (int)totalCount);
    }

    private static UserSearchCriteria BuildCriteria(CriteriaRequest request)
    {
        string? role = null;
        var excludeRole = false;
        string? status = null;
        string? phonePrefix = null;
        string? phoneSuffix = null;

        foreach (var filter in request.Filters)
        {
            switch (filter.Field.ToLowerInvariant())
            {
                case "role":
                    role = filter.Value?.GetString();
                    excludeRole = filter.Operator == CriteriaOperator.Ne;
                    break;
                case "status":
                    status = filter.Value?.GetString();
                    break;
                case "phone":
                    var phoneValue = PhoneNormalizer.Normalize(filter.Value?.GetString());
                    if (filter.Operator == CriteriaOperator.StartsWith)
                        phonePrefix = phoneValue;
                    else if (filter.Operator == CriteriaOperator.EndsWith)
                        phoneSuffix = phoneValue;
                    break;
            }
        }

        var sort = request.Sorts.FirstOrDefault();

        return new UserSearchCriteria(
            request.Keyword,
            role,
            excludeRole,
            status,
            phonePrefix,
            phoneSuffix,
            sort?.Field,
            sort?.Direction == SortDirection.Desc,
            request.Page,
            request.PageSize);
    }
}
