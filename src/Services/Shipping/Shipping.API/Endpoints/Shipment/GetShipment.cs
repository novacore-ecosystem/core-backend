using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Shipping.Application.Abstractions.Persistence.Shipments;

namespace NovaCore.Shipping.API.Endpoints.Shipment;

/// <summary>
/// The single endpoint of the foundation phase. It exists to prove the whole vertical resolves -
/// Carter module discovery, JWT/authorization, DI down to the Persistence Read Service, Swagger
/// generation - without implying that any shipping workflow is implemented yet. Real endpoints
/// (create shipment, plan transportation, record tracking, ...) arrive with their own CQRS
/// handlers in later phases; see docs/services/shipping-service.md.
/// </summary>
public sealed class GetShipmentEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Shipment Details",
        "",
        "Retrieves a shipment by its id, with its items, timeline events and packages.",
        "",
        "### Route Parameters",
        "- **shipmentId**: Unique identifier of the shipment (required, must be a valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Shipment not found",
        "- **400**: Invalid shipmentId format",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/shipments/{shipmentId}", Handle)
            .WithTags("Shipment")
            .RequireAuthorization()
            .WithName("GetShipment")
            .WithDisplayName("Get Shipment API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetShipmentResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid shipmentId,
        [FromServices] IShipmentReadService shipmentReadService,
        CancellationToken ct = default)
    {
        var shipment = await shipmentReadService.GetByIdAsync(shipmentId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Shipments.Shipment), shipmentId);

        var response = new GetShipmentResponse(
            shipment.Id,
            shipment.ShipmentNumber.Value,
            shipment.ShipmentType.ToString(),
            shipment.SourceType.ToString(),
            shipment.SourceReferenceId,
            shipment.Status.ToString(),
            shipment.ReceiverName,
            shipment.ReceiverAddress.ToString(),
            shipment.Items.Count,
            shipment.CreatedAt);

        return Results.Ok(ApiResponse<GetShipmentResponse>.Ok(response));
    }
}

/// <summary>
/// Declared here rather than in Application/Features/Shipments/DTOs on purpose: this is the
/// wiring-verification endpoint's own response shape, not a feature contract. The first real
/// query handler brings its own DTO into the Application layer where it belongs.
/// </summary>
public sealed record GetShipmentResponse(
    Guid Id,
    string ShipmentNumber,
    string ShipmentType,
    string SourceType,
    Guid SourceReferenceId,
    string Status,
    string ReceiverName,
    string ReceiverAddress,
    int ItemCount,
    DateTime CreatedAt);
