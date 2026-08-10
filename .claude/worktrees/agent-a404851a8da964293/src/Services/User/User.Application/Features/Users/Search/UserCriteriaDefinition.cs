using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Strategies;

namespace NovaCore.User.Application.Features.Users.Search;

/// <summary>
/// Admin search request-shape whitelist for <see cref="UserReadModel"/> - field names, allowed
/// operators, and sortability. Built once (static singleton) - no per-request reflection scan.
///
/// Since the 2026-07-28 Elasticsearch cutover (see docs/reference/search.md,
/// docs/tasks/2026-07-28/Task10_cutover-searchusers-to-elasticsearch.md), this definition is
/// used ONLY for request-shape validation (CriteriaRequestValidator, in SearchUsersValidator) -
/// SearchUsersHandler no longer executes a query against it (no ApplyCriteria/Postgres path
/// remains). Allowed operators here must stay in sync with what SearchUsersHandler.BuildCriteria
/// actually implements, or a filter would validate successfully and then be silently ignored.
/// FirstName/MiddleName/LastName are deliberately no longer individually filterable - the unified
/// `keyword` search (accent + case + word-order insensitive, via SearchName) supersedes them.
/// </summary>
public static class UserCriteriaDefinition
{
    public static readonly CriteriaDefinition<UserReadModel> Instance = CriteriaDefinition<UserReadModel>.Create()
        .Field(x => x.UserName).String().Sortable().AllowOperators()
        .Field(x => x.Email).String().Sortable().AllowOperators()
        .Field(x => x.Status).Enum()
        .Field(x => x.PhoneNumber, name: "phone").UsePhoneSearch(x => x.PhoneSearch, x => x.PhoneReverse)
            .AllowOperators(CriteriaOperator.StartsWith, CriteriaOperator.EndsWith)
        .Field(x => x.CreatedAt).DateTime().Sortable().AllowOperators()
        .Field(x => x.UpdatedAt).DateTime().Sortable().AllowOperators()
        // Not Sortable() - Roles is multi-valued, sorting by an array column isn't a coherent request.
        .Field(x => x.Roles, name: "role").UseStrategy(
            new StringCollectionContainsStrategy<UserReadModel>(x => x.Roles), CriteriaOperator.Eq, CriteriaOperator.Ne)
        .Build();
}
