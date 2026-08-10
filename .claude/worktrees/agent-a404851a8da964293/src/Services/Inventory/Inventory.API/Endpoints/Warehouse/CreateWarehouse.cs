using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.Inventory.Application.Features.Warehouses.Commands.CreateWarehouse;
using NovaCore.Inventory.Domain.Enums;

namespace NovaCore.Inventory.API.Endpoints.Warehouse;

public sealed record CreateWarehouseRequest(
    string Code,
    string Name,
    WarehouseType Type,
    string Country,
    string StateOrProvince,
    string City,
    string District,
    string Ward,
    string Street,
    string PostalCode);

public sealed class CreateWarehouseEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Warehouse",
        "",
        "Creates a new warehouse location with complete address information.",
        "",
        "### Request Body",
        "- **Code**: Unique warehouse code (required, must be unique)",
        "- **Name**: Warehouse name (required)",
        "- **Type**: Warehouse type (required)",
        "- **Country**: Country (required)",
        "- **StateOrProvince**: State or province (optional)",
        "- **City**: City (required)",
        "- **District**: District (optional)",
        "- **Ward**: Ward (optional)",
        "- **Street**: Street address (required)",
        "- **PostalCode**: Postal code (optional)",
        "",
        "### Behavior",
        "Creates a new warehouse with a default storage zone automatically initialized.",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **409**: Warehouse code already exists",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/warehouses", Handle)
            .WithTags("Warehouse")
            .RequirePermissions(Permissions.Warehouse.Manage)
            .WithName("CreateWarehouse")
            .WithDisplayName("Create Warehouse API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateWarehouseResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateWarehouseRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateWarehouseCommand(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Type,
            request.Country.Trim(),
            request.StateOrProvince.Trim(),
            request.City.Trim(),
            request.District.Trim(),
            request.Ward.Trim(),
            request.Street.Trim(),
            request.PostalCode.Trim());

        var response = await sender.Send(command, ct);

        return Results.Created($"/warehouses/{response.WarehouseId}",
            ApiResponse<CreateWarehouseResponse>.Ok(response));
    }
}
