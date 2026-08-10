using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Product.Application.Features.ProductTags.Commands.UpdateProductTag;

namespace NovaCore.Product.API.Endpoints.ProductTag;

public sealed record UpdateProductTagRequest(string Name);

public sealed class UpdateProductTagEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Product Tag",
        "",
        "Renames a product tag.",
        "",
        "### Route Parameters",
        "- **tagId**: Unique identifier of the tag (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: ProductTag not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tags/{tagId}", Handle)
            .WithTags("ProductTag")
            .RequirePermissions(Permissions.Product.Manage)
            .WithName("UpdateProductTag")
            .WithDisplayName("Update Product Tag API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateProductTagResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid tagId,
        [FromBody] UpdateProductTagRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new UpdateProductTagCommand(tagId, request.Name.Trim()), ct);

        return Results.Ok(ApiResponse<UpdateProductTagResponse>.Ok(response));
    }
}
