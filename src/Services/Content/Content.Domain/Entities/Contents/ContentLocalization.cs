using System.Text.Json;

namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// One culture's localized payload within a specific <see cref="ContentVersion"/> - title,
/// summary, structured JSON body (Editor.js-compatible, persisted as PostgreSQL jsonb) and
/// per-language SEO metadata. This is the sole owner of editorial content: a ContentVersion
/// carries no body of its own, only the version-level lifecycle (VersionNumber/Status) shared by
/// every language in it. Unique per (VersionId, Culture) - the same culture may appear across many
/// versions of the same Content, each with its own independently-edited payload.
/// </summary>
public sealed class ContentLocalization : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    private const int MaxTitleLength = 500;
    private const int MaxSummaryLength = 1000;

    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public Guid VersionId { get; private set; }
    public ContentVersion Version { get; private set; } = default!;
    public LanguageCode Culture { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;

    /// <summary>Structured JSON document (Editor.js output), stored as PostgreSQL jsonb. The domain
    /// treats this as an opaque validated JSON string - it never parses Editor.js-specific
    /// internals (block types, etc.), only that the payload is syntactically valid JSON.</summary>
    public string Body { get; private set; } = "{}";
    public ContentMetadata Metadata { get; private set; } = new();

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentLocalization() { }

    internal static ContentLocalization Create(
        Guid contentId,
        Guid versionId,
        LanguageCode culture,
        string title,
        string summary,
        string body,
        ContentMetadata? metadata = null)
    {
        ValidateTitle(title);
        ValidateBody(body);

        return new ContentLocalization
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            VersionId = versionId,
            Culture = culture,
            Title = title,
            Summary = summary,
            Body = body,
            Metadata = metadata ?? new ContentMetadata(),
        };
    }

    internal void UpdateContent(string title, string summary, string body, ContentMetadata? metadata)
    {
        ValidateTitle(title);
        ValidateBody(body);

        Title = title;
        Summary = summary;
        Body = body;
        Metadata = metadata ?? Metadata;
    }

    public static bool IsValidTitle(string? title)
        => !string.IsNullOrWhiteSpace(title) && title.Length <= MaxTitleLength;

    public static bool IsValidSummary(string? summary)
        => summary is null || summary.Length <= MaxSummaryLength;

    public static bool IsValidBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return false;

        try
        {
            using var _ = JsonDocument.Parse(body);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw ExceptionFactory.RequiredField("Content localization title cannot be empty.");

        if (title.Length > MaxTitleLength)
            throw ExceptionFactory.ValueTooLarge($"Content localization title cannot exceed {MaxTitleLength} characters.");
    }

    private static void ValidateBody(string body)
    {
        if (!IsValidBody(body))
            throw ExceptionFactory.InvalidFormat("Content body must be a valid JSON document.");
    }
}
