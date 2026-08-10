using NovaCore.User.Application.Abstractions.Search;
using NovaCore.User.Application.Features.Users.Search;

namespace NovaCore.User.Application.Features.Users.Commands.RebuildUserSearchIndex;

/// <summary>
/// PostgreSQL -&gt; Projection Builder -&gt; Bulk Index -&gt; Elasticsearch. Reuses the exact same
/// ProjectionBuilder/IUserSearchIndexer the live sync path uses (see
/// OnUserSearchSyncRequiredHandler) - future schema changes only touch the Projection Builder,
/// not this orchestration. See docs/reference/search.md.
/// </summary>
public sealed class RebuildUserSearchIndexHandler(
    IUserReadService userReadService,
    UserSearchProjectionBuilder projectionBuilder,
    IUserSearchIndexer searchIndexer) : ICommandHandler<RebuildUserSearchIndexCommand, RebuildUserSearchIndexResponse>
{
    private const int BatchSize = 200;

    public async Task<RebuildUserSearchIndexResponse> Handle(
        RebuildUserSearchIndexCommand request, CancellationToken ct = default)
    {
        await searchIndexer.RecreateIndexAsync(ct);

        var indexed = 0;
        var skip = 0;
        IReadOnlyList<UserReadModel> batch;

        do
        {
            batch = await userReadService.GetAllAsync(skip, BatchSize, ct);
            if (batch.Count == 0)
                break;

            var documents = await projectionBuilder.BuildManyAsync(batch, ct);
            await searchIndexer.BulkIndexAsync(documents, ct);

            indexed += batch.Count;
            skip += BatchSize;
        } while (batch.Count == BatchSize);

        return new RebuildUserSearchIndexResponse(indexed);
    }
}
