using System.Text.Json;
using System.Text.RegularExpressions;

using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Storage.Seeders;

/// <summary>
/// Seeds the Article/News/Blog content types plus enough Content items across them (varied
/// Draft/Published status, one soft-deleted, some with an "en"+"vi" translation) to meaningfully
/// exercise list/filter/cursor-pagination/cursor-point/detail/draft/publish/version/localization
/// flows end to end. Bodies are Editor.js-compatible JSON, never Markdown/HTML.
/// </summary>
public sealed partial class ContentSeeder(ContentDbContext context)
{
    private static readonly Guid SystemActorId = Guid.Parse("019fc900-a1b2-7c3d-8000-000000000001");
    private static readonly LanguageCode English = LanguageCode.Create(LanguageCodeConstant.English);
    private static readonly LanguageCode Vietnamese = LanguageCode.Create(LanguageCodeConstant.Vietnamese);

    public async Task SeedAsync()
    {
        var contentTypeIds = await SeedContentTypesAsync();
        await SeedContentsAsync(contentTypeIds);
    }

    // ============================================================================
    // Content Types
    // ============================================================================

    private async Task<Dictionary<string, Guid>> SeedContentTypesAsync()
    {
        if (await context.ContentTypes.AnyAsync())
            return await context.ContentTypes.ToDictionaryAsync(t => t.Key.Value, t => t.Id);

        var article = ContentType.Create(ContentKey.Create("article"), "Article", "Long-form editorial articles.");
        var news = ContentType.Create(ContentKey.Create("news"), "News", "Short-form, timely news items.");
        var blog = ContentType.Create(ContentKey.Create("blog"), "Blog", "Blog posts and opinion pieces.");

        context.ContentTypes.AddRange(article, news, blog);
        await context.SaveChangesAsync();

        return new Dictionary<string, Guid>
        {
            ["article"] = article.Id,
            ["news"] = news.Id,
            ["blog"] = blog.Id,
        };
    }

    // ============================================================================
    // Contents
    // ============================================================================

    private async Task SeedContentsAsync(Dictionary<string, Guid> contentTypeIds)
    {
        if (await context.Contents.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var seeded = new List<ContentEntity>();

        seeded.AddRange(SeedTopicSet("article", contentTypeIds["article"], ArticleTopics, now));
        seeded.AddRange(SeedTopicSet("news", contentTypeIds["news"], NewsTopics, now));
        seeded.AddRange(SeedTopicSet("blog", contentTypeIds["blog"], BlogTopics, now));

        context.Contents.AddRange(seeded);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Builds one content type's worth of seed items. Deterministic pattern across every topic
    /// list: items 0-1 are drafts (never published), the rest are published at staggered times
    /// (newest first) so cursor pagination has a real, stable order to page through; every third
    /// published item also gets a Vietnamese translation on the same version; the very last item
    /// is soft-deleted, so delete-exclusion is exercised by the seed data itself.
    /// </summary>
    private static IEnumerable<ContentEntity> SeedTopicSet(string prefix, Guid contentTypeId, IReadOnlyList<(string Title, string Summary, string Intro, string Body, string[] Items)> topics, DateTime now)
    {
        for (var i = 0; i < topics.Count; i++)
        {
            var topic = topics[i];
            var slug = ContentSlug.Create($"{prefix}-{i + 1}-{Slugify(topic.Title)}");
            var body = BuildEditorJsBody(topic.Title, topic.Intro, topic.Body, topic.Items);

            var content = ContentEntity.Create(
                contentTypeId, slug, English, topic.Title, topic.Summary, body, SystemActorId,
                ContentVisibility.Public);

            var versionId = content.CurrentVersionId!.Value;

            if (i % 3 == 2)
            {
                var viBody = BuildEditorJsBody($"{topic.Title} (VI)", topic.Intro, topic.Body, topic.Items);
                content.UpsertLocalization(versionId, Vietnamese, $"{topic.Title} (Tieng Viet)", topic.Summary, viBody, SystemActorId);
            }

            var isDraft = i < 2;
            var isLast = i == topics.Count - 1;

            if (!isDraft)
                content.Publish(versionId, now.AddDays(-(topics.Count - i)));

            if (isLast)
                content.Delete();

            yield return content;
        }
    }

    private static string Slugify(string title)
    {
        var stripped = NonSlugCharacters().Replace(title.ToLowerInvariant(), " ");
        return string.Join('-', stripped.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    private static string BuildEditorJsBody(string heading, string intro, string body, IReadOnlyList<string> listItems)
    {
        var doc = new
        {
            time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            blocks = new object[]
            {
                new { id = Guid.NewGuid().ToString("N")[..8], type = "header", data = new { text = heading, level = 2 } },
                new { id = Guid.NewGuid().ToString("N")[..8], type = "paragraph", data = new { text = intro } },
                new { id = Guid.NewGuid().ToString("N")[..8], type = "paragraph", data = new { text = body } },
                new { id = Guid.NewGuid().ToString("N")[..8], type = "list", data = new { style = "unordered", items = listItems } },
            },
            version = "2.31.0",
        };

        return JsonSerializer.Serialize(doc);
    }

    // ============================================================================
    // Topic data
    // ============================================================================

    private static readonly (string Title, string Summary, string Intro, string Body, string[] Items)[] ArticleTopics =
    [
        ("Getting Started with NovaCore", "A guided tour of the platform for new users.", "NovaCore brings commerce, content, and communication together in one platform.", "This article walks through account setup, workspace navigation, and the first steps toward publishing your own content.", ["Create your workspace", "Invite your team", "Publish your first article"]),
        ("Understanding Content Versioning", "How versions and languages work together in the Content Platform.", "Every edit to a Content item creates a new version rather than overwriting history.", "A version groups every language's content together, so publishing a version publishes every language it carries at once.", ["Versions preserve history", "Languages live inside a version", "Publishing is atomic per version"]),
        ("A Guide to Multi-Language Publishing", "Reaching readers in more than one language.", "Translating content is a first-class workflow, not an afterthought.", "The Translation API lets an editor add a new language onto an existing version using the same mechanism draft editing already relies on.", ["Pick a source version", "Translate title, summary, and body", "Publish once, serve every language"]),
        ("Editor.js and Structured Content", "Why structured JSON beats a single blob of HTML.", "Editor.js produces block-based JSON instead of raw markup.", "Structured content is easier to search, transform, and render consistently across web and mobile clients.", ["Blocks instead of markup", "Safer to render", "Portable across clients"]),
        ("Designing a Cursor-Paginated Feed", "Why NovaCore's content feeds use cursors instead of page numbers.", "Offset pagination breaks when content changes between requests.", "Cursor pagination anchors each page to the last item seen, so a feed stays stable even as new content is published.", ["No skipped or duplicated rows", "Stable under concurrent writes", "Cursor points let the frontend jump ahead"]),
        ("Soft Delete and Content Recovery", "How NovaCore protects against accidental deletion.", "Deleting a Content item never destroys it immediately.", "A soft-deleted item is hidden from every normal read but can be restored until the retention window closes.", ["Delete hides, it does not destroy", "Restore brings it straight back", "Retention job cleans up after 7 days"]),
        ("Working with Content Types", "Article, News, and Blog all share one underlying model.", "A Content Type is a schema, not a table.", "Article, News, and Blog are all built on the same Content aggregate - there is no type-specific code anywhere in the platform.", ["One model, many types", "New types need no migration", "Fields stay consistent across types"]),
        ("Admin Search and Filtering", "Finding the right content fast in the Admin console.", "The Admin list combines the platform's Criteria filters with cursor pagination.", "Filter by content type, status, or visibility, then page through results without ever loading a full article body.", ["Filter by type and status", "Lightweight list rows", "Full detail is one click away"]),
        ("Draft, Review, and Publish", "The editorial lifecycle behind every Content item.", "Content moves from Draft through review to Published.", "A draft can keep being edited even after an earlier version has been published, so editors never block on a live article.", ["Draft is always editable", "Publishing never touches the draft", "History is never overwritten"]),
        ("Best Practices for SEO Metadata", "Getting the most out of per-language SEO fields.", "SEO metadata is set per language, not per Content.", "Each localization carries its own SEO title, meta description, and canonical URL, since search engines index every language separately.", ["Write a unique title per language", "Keep descriptions under 160 characters", "Set a canonical URL for syndicated content"]),
    ];

    private static readonly (string Title, string Summary, string Intro, string Body, string[] Items)[] NewsTopics =
    [
        ("NovaCore Content Platform Reaches General Availability", "The WCM capability is now available to every tenant.", "After several implementation phases, the Content Platform is ready for production use.", "Today's release brings content CRUD, drafts, publishing, versioning, translation, and soft delete to every tenant on the platform.", ["CRUD and drafts", "Versioning and translation", "Soft delete and restore"]),
        ("New Cursor-Point API Speeds Up Pagination", "Frontend teams can now pre-fetch jump-to-page cursors.", "A new endpoint returns only the boundary cursor for each page.", "Instead of paying for a full page fetch just to know where page 5 starts, clients can now request cursor points in the background.", ["One background call per search session", "No offset pagination needed", "Works alongside every existing filter"]),
        ("JSONB Storage Now Backs Every Content Body", "Content bodies moved from plain text to PostgreSQL jsonb.", "Editor.js output is now stored as structured JSON, not a text blob.", "The migration preserves every existing field while unlocking future JSON-aware querying and validation.", ["Structured storage", "Validated on write", "No data loss during migration"]),
        ("Hard-Delete Retention Job Now Live", "Soft-deleted content is permanently removed after 7 days.", "A new background job runs hourly to enforce the retention window.", "Anything soft-deleted for longer than the configured retention period - 7 days by default - is now permanently removed automatically.", ["Runs hourly", "7-day default retention", "Configurable per environment"]),
        ("Admin Console Adds Content Search", "Filter and page through content without loading full bodies.", "The Admin list API is live behind the existing Criteria mechanism.", "Editors can now filter by content type, status, and visibility, then cursor-paginate through results in the console.", ["Criteria-based filters", "Cursor pagination", "Lightweight list rows"]),
        ("Translation Workflow Ships for Editors", "Add a language to any version without leaving the editor.", "The Translation API reuses the same upsert mechanism as draft editing.", "Editors can now translate an existing version into a new language, or update an existing translation, from one endpoint.", ["One upsert mechanism", "Works on any draft version", "Existing translations update in place"]),
        ("Landing Page Feed API Announced", "A public, cursor-paginated feed of published content.", "The Landing API returns only published, non-deleted content.", "Built for the marketing site's landing page, the feed resolves each item to the visitor's language automatically.", ["Published-only", "Language fallback built in", "No draft or version history exposed"]),
        ("Restore API Brings Back Deleted Content", "Undo an accidental delete without losing history.", "A soft-deleted Content item can be restored at any point before hard deletion.", "Restoring a Content item returns it to exactly the state it was in before deletion - no data is recreated or guessed.", ["Restores the exact prior state", "Available until hard-delete runs", "Identity never changes"]),
    ];

    private static readonly (string Title, string Summary, string Intro, string Body, string[] Items)[] BlogTopics =
    [
        ("Why We Rebuilt Content Versioning Around Language", "A behind-the-scenes look at a core domain decision.", "The old model tied a language to an entire version lineage.", "We moved the content body onto a Version+Language join so a single version can carry every language at once - simpler to reason about, and closer to how editors actually work.", ["One version, many languages", "Simpler mental model", "Matches real editorial workflows"]),
        ("Choosing Editor.js Over a Rich Text Blob", "The tradeoffs behind a structured content editor.", "A single HTML or Markdown blob is hard to validate and transform.", "Editor.js gives us block-level structure without locking the domain layer into any particular editor's internals.", ["Structure without lock-in", "Easier to validate", "Portable across renderers"]),
        ("Pragmatic Cursor Pagination, Not a Distributed Snapshot", "Why we didn't build a search-session snapshot system.", "Perfect consistency across pages isn't worth the complexity here.", "A record moving between pages because content changed mid-session is an acceptable tradeoff for a WCM admin or landing feed.", ["Good enough beats perfect", "No snapshot infrastructure needed", "Matches how real search UIs already behave"]),
        ("Soft Delete: A Safety Net, Not a Trash Can", "Designing delete so mistakes are recoverable.", "Deleting content is common, and mistakes happen.", "Soft delete plus a 7-day hard-delete job gives editors a real safety net without keeping deleted rows around forever.", ["Recoverable by default", "Time-boxed retention", "No manual cleanup required"]),
        ("Keeping the Admin List Lightweight", "How we kept list screens fast as content grew.", "Loading full Editor.js bodies for a list view doesn't scale.", "The Admin and Landing list queries project only the fields a list screen needs, never the body, straight from the database.", ["Project, don't over-fetch", "Body loads only on detail view", "Scales with content volume"]),
        ("One Domain Model for Article, News, and Blog", "Why we didn't build separate tables per content type.", "Article, News, and Blog are the same shape underneath.", "Every content type in NovaCore is built on the same Content aggregate - a new type is a row in ContentType, not a migration.", ["No per-type tables", "New types ship without code changes", "Consistent behavior everywhere"]),
        ("Translating the Upsert Mechanism Into a Feature", "Reusing one domain method for two very different APIs.", "Draft editing and translation looked like two features at first.", "Both turned out to be the same operation - upsert a language's content onto a version - so we exposed one domain method behind two thin API-level commands.", ["Same mechanism, two intents", "No duplicated business logic", "Fewer places to introduce bugs"]),
        ("What We Learned Shipping the First WCM Milestone", "Retrospective notes from the initial Content Platform release.", "Building CRUD, drafts, publish, versioning, and translation together surfaced real tradeoffs.", "Keeping the domain model honest about what a 'version' and a 'language' actually mean paid off across every feature we built on top of it.", ["Get the core model right first", "Reuse beats parallel mechanisms", "Lightweight reads matter as much as correct writes"]),
    ];
}
