using NovaCore.BuildingBlock.Search.Abstractions;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport.Products.Elasticsearch;

namespace NovaCore.BuildingBlock.Search.Indexing;

/// <summary>
/// Every "index name" a caller passes in here is actually an ES alias, not a concrete index - the
/// real index behind it is versioned (`{alias}-{timestamp}`) and swapped atomically via
/// Indices.UpdateAliases (add new + remove old in one call), so a mapping change never leaves the
/// alias resolving to nothing, unlike a blocking drop+create. Callers
/// (IProductSearchIndexer/IUserSearchIndexer) are unaffected - same alias string in, same
/// read/write behavior out; write ops (Index/Delete/BulkIndex) target the alias directly, which ES
/// fully supports as long as exactly one concrete index is behind it (guaranteed by the swap logic
/// below).
/// </summary>
public sealed class ElasticsearchIndexer<TDocument>(ElasticsearchClient client) : IElasticsearchIndexer<TDocument>
    where TDocument : class
{
    public Task EnsureIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default) =>
        EnsureIndexCoreAsync(indexName, configureMapping, null, ct);

    public Task EnsureIndexAsync(
        string indexName,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>> configureSettings,
        CancellationToken ct = default) =>
        EnsureIndexCoreAsync(indexName, configureMapping, configureSettings, ct);

    public Task RecreateIndexAsync(string indexName, Action<PropertiesDescriptor<TDocument>> configureMapping, CancellationToken ct = default) =>
        RecreateIndexCoreAsync(indexName, configureMapping, null, ct);

    public Task RecreateIndexAsync(
        string indexName,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>> configureSettings,
        CancellationToken ct = default) =>
        RecreateIndexCoreAsync(indexName, configureMapping, configureSettings, ct);

    private async Task EnsureIndexCoreAsync(
        string alias,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>>? configureSettings,
        CancellationToken ct)
    {
        var aliasExists = await client.Indices.ExistsAliasAsync(alias, ct);
        if (aliasExists.Exists)
            return;

        await MigrateLegacyConcreteIndexIfPresentAsync(alias, ct);

        await CreateIndexWithAliasAsync(VersionedIndexName(alias), alias, configureMapping, configureSettings, ct);
    }

    private async Task RecreateIndexCoreAsync(
        string alias,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>>? configureSettings,
        CancellationToken ct)
    {
        await MigrateLegacyConcreteIndexIfPresentAsync(alias, ct);

        var previousIndices = await GetConcreteIndicesBehindAliasAsync(alias, ct);
        var newIndex = VersionedIndexName(alias);

        await CreateIndexAsync(newIndex, configureMapping, configureSettings, ct);

        var swapResponse = await client.Indices.UpdateAliasesAsync(
            u => u.Actions(BuildAliasSwapActions(alias, newIndex, previousIndices)), ct);
        EnsureSuccess(swapResponse, $"swap alias '{alias}' from {(previousIndices.Count == 0 ? "(none)" : string.Join(", ", previousIndices))} to '{newIndex}'");

        // Best-effort: the alias already points only at the new index once the swap above
        // succeeds, so a failure to delete the old generation here is not fatal - it's just a
        // dangling index to clean up later, never a source of stale search results.
        foreach (var previousIndex in previousIndices)
            await client.Indices.DeleteAsync(previousIndex, ct);
    }

    public async Task IndexAsync(string indexName, string documentId, TDocument document, CancellationToken ct = default)
    {
        var response = await client.IndexAsync(document, indexName, documentId, ct);
        EnsureSuccess(response, $"index document '{documentId}' into '{indexName}'");
    }

    public async Task DeleteAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        // A delete for a document that no longer exists is a valid outcome for a read-model
        // sync (e.g. a redelivered event) - not surfaced as a failure.
        await client.DeleteAsync(indexName, documentId, ct);
    }

    public async Task BulkIndexAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken ct = default)
    {
        var items = documents.ToList();
        if (items.Count == 0)
            return;

        var response = await client.BulkAsync(b =>
        {
            b.Index(indexName);
            foreach (var item in items)
                b.Index(item.Document, op => op.Id(item.Id));
        }, ct);

        if (response.Errors)
        {
            throw new InvalidOperationException(
                $"Bulk index into '{indexName}' failed for {response.ItemsWithErrors.Count()} document(s): {response.DebugInformation}");
        }
    }

    /// <summary>
    /// Handles the one-time upgrade from this codebase's pre-alias state, where the literal name
    /// now used as an alias was a plain concrete index. An alias and a concrete index can't share a
    /// name in ES, so encountering one here means it predates this change. No environment has ever
    /// run this code against a live Elasticsearch with real data in it (see
    /// docs/reference/search.md's "Operational note"), so dropping it outright - rather than
    /// building reindex-migration machinery for a state this repo's history never actually
    /// reached - is the correct amount of handling, not a shortcut around real data loss.
    /// </summary>
    private async Task MigrateLegacyConcreteIndexIfPresentAsync(string alias, CancellationToken ct)
    {
        // Indices.ExistsAsync(name) resolves aliases too - it returns true once `alias` is
        // already alias-managed, not just for a genuine pre-Task-20 concrete index. Without this
        // check, every later call (e.g. a rebuild after the alias already exists) would
        // mis-detect "legacy" and try to DELETE the alias name directly, which ES rejects
        // (400 illegal_argument_exception: "matches an alias, specify the corresponding concrete
        // indices instead").
        var aliasExists = await client.Indices.ExistsAliasAsync(alias, ct);
        if (aliasExists.Exists)
            return;

        var concreteIndexExists = await client.Indices.ExistsAsync(alias, ct);
        if (!concreteIndexExists.Exists)
            return;

        var deleteResponse = await client.Indices.DeleteAsync(alias, ct);
        EnsureSuccess(deleteResponse, $"drop legacy pre-alias index '{alias}'");
    }

    private async Task<IReadOnlyList<string>> GetConcreteIndicesBehindAliasAsync(string alias, CancellationToken ct)
    {
        var aliasExists = await client.Indices.ExistsAliasAsync(alias, ct);
        if (!aliasExists.Exists)
            return [];

        var response = await client.Indices.GetAliasAsync((Elastic.Clients.Elasticsearch.Names)alias, ct);
        EnsureSuccess(response, $"resolve concrete index(es) behind alias '{alias}'");

        // response.Values is nullable-oblivious (external, unannotated assembly) - EnsureSuccess
        // above already guarantees a valid response, which always carries a Values dictionary.
#pragma warning disable CS8602
        return response.Values.Keys.Select(name => name.ToString()).ToList();
#pragma warning restore CS8602
    }

    private static Action<IndexUpdateAliasesActionDescriptor>[] BuildAliasSwapActions(
        string alias, string newIndex, IReadOnlyList<string> previousIndices)
    {
        var actions = new List<Action<IndexUpdateAliasesActionDescriptor>>
        {
            a => a.Add(add => add.Index(newIndex).Alias(alias)),
        };

        foreach (var previousIndex in previousIndices)
            actions.Add(a => a.Remove(rm => rm.Index(previousIndex).Alias(alias)));

        return [.. actions];
    }

    private static string VersionedIndexName(string alias) =>
        $"{alias}-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";

    private async Task CreateIndexAsync(
        string indexName,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>>? configureSettings,
        CancellationToken ct)
    {
        var response = await client.Indices.CreateAsync<TDocument>(indexName, c =>
        {
            if (configureSettings is not null)
                c.Settings(configureSettings);
            c.Mappings(m => m.Properties(configureMapping));
        }, ct);
        EnsureSuccess(response, $"create index '{indexName}'");
    }

    private async Task CreateIndexWithAliasAsync(
        string indexName,
        string alias,
        Action<PropertiesDescriptor<TDocument>> configureMapping,
        Action<IndexSettingsDescriptor<TDocument>>? configureSettings,
        CancellationToken ct)
    {
        var response = await client.Indices.CreateAsync<TDocument>(indexName, c =>
        {
            c.AddAlias(alias);
            if (configureSettings is not null)
                c.Settings(configureSettings);
            c.Mappings(m => m.Properties(configureMapping));
        }, ct);
        EnsureSuccess(response, $"create index '{indexName}' with alias '{alias}'");
    }

    private static void EnsureSuccess(ElasticsearchResponse response, string action)
    {
        if (!response.IsValidResponse)
            throw new InvalidOperationException($"Elasticsearch operation failed to {action}: {response.DebugInformation}");
    }
}
