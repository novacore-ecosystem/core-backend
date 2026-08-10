using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Infrastructure.Idempotency;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using NovaCore.Order.Application.Features.Orders.Commands.CreateOrder;
using NovaCore.Order.Application.Features.Orders.DTOs;

namespace NovaCore.Order.API.Endpoints.Order;

public sealed record CreateOrderOwnerInfoRequest(
    string OwnerName,
    string OwnerEmail,
    string OwnerPhone);

public sealed record CreateOrderShippingInfoRequest(
    string ShippingMethod,
    string ReceiverName,
    string ReceiverPhone,
    string ShippingAddress,
    string Note);

public sealed record CreateOrderItemRequestDto(
    Guid ProductId,
    Guid VariationId,
    int Quantity);

public sealed record CreateOrderRequest(
    CreateOrderOwnerInfoRequest OwnerInfo,
    CreateOrderShippingInfoRequest ShippingInfo,
    IReadOnlyCollection<CreateOrderItemRequestDto> Items);

public sealed class CreateOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Order",
        "",
        "Creates a new order for the calling user. The ordering customer is always the caller's",
        "own identity (from the auth token) - see POST /orders/admin for creating an order on",
        "behalf of another user. Before creating anything, Items is compared against the caller's",
        "current server-side cart (GET /cart) - a mismatch (quantity, availability, or presence",
        "differs) is rejected with 409 so the client can refresh and resubmit. On success the cart",
        "is cleared and a Pending order is persisted; inventory deduction and confirmation continue",
        "asynchronously via the CreateOrder saga. Poll GET /orders/{orderId} or listen for realtime",
        "updates to observe the order reach Confirmed or Cancelled.",
        "",
        "### Request Body",
        "- **CustomerName**: Snapshot of the customer's display name, captured once and never resynced (required)",
        "- **CustomerPhone**: Snapshot of the customer's phone number, captured once and never resynced (required)",
        "- **ShippingAddress**: Free-text shipping address, same snapshot convention as CustomerName/CustomerPhone (required)",
        "- **Items**: List of order items (required, at least one item, max 50, no duplicates) - must match the caller's current cart exactly",
        "  - **ProductId**: Id of the product (required)",
        "  - **VariationId**: Id of the product variation being ordered (required, must exist and be Active in the local product catalog)",
        "  - **Quantity**: Quantity requested (required, 1-100)",
        "  - **Discount**: Optional flat discount applied to the line (default 0, cannot exceed the line's pre-discount total)",
        "",
        "### Error Responses",
        "- **400**: Invalid request, validation failed, a product is not orderable, or stock is insufficient",
        "- **404**: Product not found in the local product catalog",
        "- **409**: Items don't match the caller's current cart - refresh and resubmit",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", Handle)
            .WithTags("Order")
            .RequireAuthorization()
            .Headers([
                new HeaderDefinition(HeaderKeyConstant.IdempotencyKey, true, "Ensures this order is only created once, even if the request is retried")
            ])
            .RequireIdempotency()
            .WithName("CreateOrder")
            .WithDisplayName("Create Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateOrderRequest request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedAccessException("User must be authenticated to create an order.");

        var command = new CreateOrderCommand(
            MapToOrderOwnerRequestDto(userId, request.OwnerInfo),
            MapToOrderShippingInfoRequestDto(request.ShippingInfo),
            MapToOrderItemRequestDtos(request.Items));

        var response = await sender.Send(command, ct);

        return Results.Accepted($"/orders/{response.OrderId}",
            ApiResponse<CreateOrderResponse>.Ok(response));
    }

    private static OrderOwnerRequestDto MapToOrderOwnerRequestDto(
        Guid ownerId,
        CreateOrderOwnerInfoRequest input)
    {
        return new OrderOwnerRequestDto(
            ownerId,
            input.OwnerName.Trim(),
            Email.Create(input.OwnerEmail.Trim()),
            PhoneNumber.Create(input.OwnerPhone.Trim()));
    }

    private static OrderShippingInfoRequestDto MapToOrderShippingInfoRequestDto(
        CreateOrderShippingInfoRequest input)
    {
        return new OrderShippingInfoRequestDto(
            Enum.TryParse<ShippingMethod>(input.ShippingMethod.Trim(), out var shippingMethod)
                ? shippingMethod
                : throw new ArgumentException("Invalid shipping method"),
            input.ReceiverName.Trim(),
            PhoneNumber.Create(input.ReceiverPhone.Trim()),
            input.ShippingAddress.Trim(),
            input.Note.Trim());
    }

    private static OrderItemRequestDto[] MapToOrderItemRequestDtos(
        IReadOnlyCollection<CreateOrderItemRequestDto> items)
    {
        return [.. items.Select(i =>
            new OrderItemRequestDto(i.ProductId, i.VariationId, i.Quantity))];
    }
}
