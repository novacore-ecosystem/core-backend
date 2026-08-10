using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Warehouses.Queries.GetWarehouse;

namespace NovaCore.Inventory.API.Endpoints.Warehouse;

public sealed class GetWarehouseEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Warehouse Details",
        "",
        "Retrieves warehouse information by warehouse ID.",
        "",
        "### Route Parameters",
        "- **warehouseId**: Unique identifier of the warehouse (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Warehouse not found",
        "- **400**: Invalid warehouseId format",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/warehouses/{warehouseId}", Handle)
            .WithTags("Warehouse")
            .RequirePermissions(Permissions.Warehouse.View)
            .WithName("GetWarehouse")
            .WithDisplayName("Get Warehouse API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetWarehouseResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid warehouseId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetWarehouseQuery(warehouseId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetWarehouseResponse>.Ok(response));
    }
}
