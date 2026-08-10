using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Abstractions.Search;
using NovaCore.Product.Application.Features.Products.Search;

namespace NovaCore.Product.Application.Features.Products.Commands.RebuildProductSearchIndex;

/// <summary>
/// PostgreSQL -&gt; Projection Builder -&gt; Bulk Index -&gt; Elasticsearch. Reuses the exact same
/// ProjectionBuilder/IProductSearchIndexer the live sync path uses (see
/// OnProductSearchSyncRequiredHandler) - future schema changes only touch the Projection
/// Builder, not this orchestration. See docs/reference/search.md.
/// </summary>
public sealed class RebuildProductSearchIndexHandler(
    IProductReadService productReadService,
    IProductSearchIndexer searchIndexer) : ICommandHandler<RebuildProductSearchIndexCommand, RebuildProductSearchIndexResponse>
{
    private const int BatchSize = 200;

    public async Task<RebuildProductSearchIndexResponse> Handle(
        RebuildProductSearchIndexCommand request, CancellationToken ct = default)
    {
        await searchIndexer.RecreateIndexAsync(ct);

        var indexed = 0;
        var skip = 0;
        ProductEntity[] batch;

        do
        {
            batch = await productReadService.GetAllAsync(skip, BatchSize, ct);
            if (batch.Length == 0)
                break;

            var documents = await ProductSearchProjectionBuilder.BuildManyAsync(batch, ct);
            await searchIndexer.BulkIndexAsync(documents, ct);

            indexed += batch.Length;
            skip += BatchSize;
        } while (batch.Length == BatchSize);

        return new RebuildProductSearchIndexResponse(indexed);
    }
}
