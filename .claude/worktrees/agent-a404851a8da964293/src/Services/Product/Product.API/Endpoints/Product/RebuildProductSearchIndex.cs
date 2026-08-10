using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.Products.Commands.RebuildProductSearchIndex;

namespace NovaCore.Product.API.Endpoints.Product;

/// <summary>PostgreSQL -&gt; Projection Builder -&gt; Bulk Index -&gt; Elasticsearch. See docs/reference/search.md.</summary>
public sealed class RebuildProductSearchIndexEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Rebuild Product Search Index",
        "",
        "Drops and recreates the Elasticsearch product-search index, then repopulates it entirely",
        "from PostgreSQL (the source of truth). Use after a schema change or to recover from an",
        "out-of-sync index.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/search/rebuild", Handle)
            .WithTags("Product")
            .RequirePermissions(Permissions.Product.Reindex)
            .WithName("RebuildProductSearchIndex")
            .WithDisplayName("Rebuild Product Search Index API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RebuildProductSearchIndexResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new RebuildProductSearchIndexCommand(), ct);

        return Results.Ok(ApiResponse<RebuildProductSearchIndexResponse>.Ok(response));
    }
}
