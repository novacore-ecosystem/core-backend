using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;

namespace NovaCore.BuildingBlock.Search.Abstractions;

/// <summary>
/// The one reusable component allowed to write to Elasticsearch. Generic over the document
/// type so any future service's read-model document can reuse it without new BuildingBlock
/// code - query-side access is intentionally a separate, per-service concern (see the task's
/// Search Repository requirement), never mixed into this interface.
///
/// Every <c>indexName</c> parameter below is actually an ES alias (since 2026-07-28, Task 20's
/// alias-based blue/green reindexing) - the real, versioned index behind it is managed
/// internally by the implementation. Callers pass the same literal name they always have; reads
/// and writes against that name behave exactly as if it were a concrete index, since ES resolves
/// an alias to its single backing index transparently.
/// </summary>
public interface IElasticsearchIndexer<TDocument> where TDocument : class
{
    /// <summary>Creates the index (and its alias) with the given mapping only if the alias doesn't already exist. Safe to call on every service startup.</summary>
    Task EnsureIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default);

    /// <summary>
    /// Overload accepting index settings (e.g. a custom analyzer) alongside the mapping - the escape
    /// hatch for services that need more than field-level mapping (e.g. accent-insensitive search).
    /// Additive: does not change behavior for existing callers of the 3-arg overload above.
    /// </summary>
    Task EnsureIndexAsync(
        string indexName,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>> configureSettings,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a new versioned index with the given mapping and atomically swaps the alias to
    /// point at it (add-new + remove-old in one call), then deletes the old generation - used by
    /// rebuild flows, never by the live sync path. Never leaves the alias resolving to nothing,
    /// unlike a blocking drop+create.
    /// </summary>
    Task RecreateIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default);

    /// <summary>See <see cref="EnsureIndexAsync(string, Action{PropertiesDescriptor{TDocument}}, Action{IndexSettingsDescriptor{TDocument}}, CancellationToken)"/>.</summary>
    Task RecreateIndexAsync(
        string indexName,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>> configureSettings,
        CancellationToken ct = default);

    Task IndexAsync(string indexName, string documentId, TDocument document, CancellationToken ct = default);

    Task DeleteAsync(string indexName, string documentId, CancellationToken ct = default);

    Task BulkIndexAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken ct = default);
}
