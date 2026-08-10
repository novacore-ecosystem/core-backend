using NovaCore.User.Application.Abstractions.Search;
using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Search;

/// <summary>
/// The Projection Builder: UserReadModel -&gt; UserSearchDocument. The only place a UserSearchDocument
/// is assembled - both the live sync path and the rebuild path call into it, so a future schema
/// change touches exactly one class. See docs/reference/search.md.
/// </summary>
public sealed class UserSearchProjectionBuilder(IUserDisplayNameFormatter displayNameFormatter)
{
    // The index stores one document per user, not one per requesting caller's locale - search
    // results always display the default-locale name, independent of whoever is searching. See
    // docs/tasks/2026-07-28/Task8_projection-builder-and-sync-events.md.
    private const string IndexDisplayLocale = "en";

    public Task<UserSearchDocument> BuildAsync(UserReadModel user, CancellationToken ct = default) =>
        Task.FromResult(Build(user));

    public Task<IReadOnlyList<UserSearchDocument>> BuildManyAsync(IReadOnlyList<UserReadModel> users, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<UserSearchDocument>>(users.Select(Build).ToList());

    private UserSearchDocument Build(UserReadModel user) => new()
    {
        UserId = user.Id,
        FirstName = user.FirstName,
        MiddleName = user.MiddleName,
        LastName = user.LastName,
        DisplayName = displayNameFormatter.Format(user.FirstName, user.MiddleName, user.LastName, IndexDisplayLocale),
        SearchName = BuildSearchName(user.FirstName, user.MiddleName, user.LastName),
        UserName = user.UserName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        PhoneSearch = user.PhoneSearch,
        PhoneReverse = user.PhoneReverse,
        Roles = user.Roles,
        Status = user.Status.ToString(),
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
    };

    // Word order doesn't matter here - the index-side analyzer tokenizes this field, and a
    // multi-match query matches documents containing the query's terms in any order. Only
    // case/accent-folding (done by the analyzer, not here) and whitespace-collapse are this
    // method's job.
    private static string BuildSearchName(string firstName, string middleName, string lastName) =>
        string.Join(' ', new[] { firstName, middleName, lastName }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim()));
}
