using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;

using NovaCore.User.Application.Abstractions.Search;

namespace NovaCore.User.Persistence.Contexts.Users.Search.Mapping;

/// <summary>
/// The User Search index settings + field mapping - the only place UserSearchDocument's ES
/// schema is defined. Future schema changes only touch this file + UserSearchProjectionBuilder.
///
/// SearchNameAnalyzer is the one piece of this whole codebase with no prior Elasticsearch
/// precedent to copy: a standard tokenizer plus the built-in "lowercase" and "asciifolding"
/// token filters, giving case- AND accent-insensitive matching (Product's Name/CategoryNames/etc.
/// only ever got case-insensitivity for free from the default "standard" analyzer - accent
/// folding needed this custom analyzer to be added at all). Word-order independence (matching
/// "Van A" against "Nguyen Van A") falls out of a plain multi-match query's own term-based OR
/// semantics, not anything analyzer-specific - see UserSearchRepository.
///
/// DisplayName is intentionally NOT analyzed the same way - it's a plain Keyword, so the exact
/// display casing/accents a user sees in search results are never altered by folding.
/// See docs/reference/search.md and docs/tasks/2026-07-28/Task7_search-document-and-accent-insensitive-mapping.md.
/// </summary>
public static class UserSearchIndexMapping
{
    public const string SearchNameAnalyzer = "user_search_name_analyzer";

    public static void ConfigureSettings(IndexSettingsDescriptor<UserSearchDocument> settings)
    {
        settings.Analysis(a => a
            .Analyzers(an => an
                .Custom(SearchNameAnalyzer, c => c
                    .Tokenizer("standard")
                    .Filter(["lowercase", "asciifolding"]))));
    }

    public static void Configure(PropertiesDescriptor<UserSearchDocument> properties)
    {
        properties
            .Keyword(d => d.UserId)
            .Keyword(d => d.FirstName, k => k.Index(false))
            .Keyword(d => d.MiddleName, k => k.Index(false))
            .Keyword(d => d.LastName, k => k.Index(false))
            .Keyword(d => d.DisplayName)
            .Text(d => d.SearchName, t => t.Analyzer(SearchNameAnalyzer))
            .Text(d => d.UserName, t => t.Fields(f => f.Keyword("keyword")))
            .Text(d => d.Email, t => t.Fields(f => f.Keyword("keyword")))
            .Keyword(d => d.PhoneNumber)
            .Keyword(d => d.PhoneSearch)
            .Keyword(d => d.PhoneReverse)
            .Keyword(d => d.Roles)
            .Keyword(d => d.Status)
            .Date(d => d.CreatedAt)
            .Date(d => d.UpdatedAt);
    }
}
