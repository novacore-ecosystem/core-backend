namespace NovaCore.User.Application.Abstractions.Search;

/// <summary>
/// The Elasticsearch read-model document for User Search - deliberately not the UserProfile
/// aggregate. DisplayName is the exact, unfolded formatted name (for showing search results);
/// SearchName is a separate field carrying the same name parts through an accent/case-folding
/// analyzer purely for matching - never returned for display. See docs/reference/search.md and
/// docs/tasks/2026-07-28/Task7_search-document-and-accent-insensitive-mapping.md.
/// </summary>
public sealed class UserSearchDocument
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string SearchName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhoneSearch { get; set; } = string.Empty;
    public string PhoneReverse { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
