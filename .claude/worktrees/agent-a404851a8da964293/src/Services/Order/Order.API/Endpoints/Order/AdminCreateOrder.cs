using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Order.Application.Features.Orders.Commands.CreateOrder;
using NovaCore.Order.Application.Features.Orders.DTOs;

namespace NovaCore.Order.API.Endpoints.Order;

public sealed record AdminCreateOrderOwnerInfoRequest(
    Guid OwnerId,
    string OwnerName,
    string OwnerEmail,
    string OwnerPhone);

public sealed record AdminCreateOrderShippingInfoRequest(
    string ShippingMethod,
    string ReceiverName,
    string ReceiverPhone,
    string ShippingAddress);

public sealed record AdminCreateOrderItemRequestDto(
    Guid ProductId,
    Guid VariationId,
    int Quantity,
    decimal Discount = 0m);

public sealed record AdminCreateOrderRequest(
    AdminCreateOrderOwnerInfoRequest OwnerInfo,
    AdminCreateOrderShippingInfoRequest ShippingInfo,
    IReadOnlyCollection<AdminCreateOrderItemRequestDto> Items);

public sealed class AdminCreateOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Admin Create Order",
        "",
        "Admin/Root only. Creates an order on behalf of an explicitly specified customer - unlike",
        "POST /orders, there's no \"current user's cart\" to source or diff against, so items are",
        "taken as given (still validated against the product catalog and stock, same as the client",
        "path) and the customer's cart is never touched.",
        "",
        "### Request Body",
        "- **CustomerId**: Id of the customer the order is being created for (required)",
        "- **CustomerName**: Snapshot of the customer's display name, captured once and never resynced (required)",
        "- **CustomerPhone**: Snapshot of the customer's phone number, captured once and never resynced (required)",
        "- **ShippingAddress**: Free-text shipping address, same snapshot convention as CustomerName/CustomerPhone (required)",
        "- **Items**: List of order items (required, at least one item, max 50, no duplicates)",
        "  - **ProductId**: Id of the product (required)",
        "  - **VariationId**: Id of the product variation being ordered (required, must exist and be Active in the local product catalog)",
        "  - **Quantity**: Quantity requested (required, 1-100)",
        "  - **Discount**: Optional flat discount applied to the line (default 0, cannot exceed the line's pre-discount total)",
        "",
        "### Error Responses",
        "- **400**: Invalid request, validation failed, a product is not orderable, or stock is insufficient",
        "- **403**: Caller is not Admin/Root",
        "- **404**: Product not found in the local product catalog",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/admin", Handle)
            .WithTags("Order")
            .RequirePermissions(Permissions.Order.CreateOnBehalf)
            .WithName("AdminCreateOrder")
            .WithDisplayName("Admin Create Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateOrderResponse>>(StatusCodes.Status202Accepted);
    }

    private static async Task<IResult> Handle(
        [FromBody] AdminCreateOrderRequest request,
        [FromServices] ICurrentUserService currentUser,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var adminId = currentUser.GetUserId()
            ?? throw new UnauthorizedAccessException(
                "Admin user must be authenticated to create an order on behalf of a customer.");

        var command = new CreateOrderCommand(
            MapToOrderOwnerRequestDto(request.OwnerInfo),
            MapToOrderShippingInfoRequestDto(request.ShippingInfo),
            MapToOrderItemRequestDtos(request.Items),
            adminId);
        var response = await sender.Send(command, ct);

        return Results.Accepted($"/orders/{response.OrderId}",
            ApiResponse<CreateOrderResponse>.Ok(response));
    }

    private static OrderOwnerRequestDto MapToOrderOwnerRequestDto(
        AdminCreateOrderOwnerInfoRequest input)
    {
        return new OrderOwnerRequestDto(
            input.OwnerId,
            input.OwnerName.Trim(),
            Email.Create(input.OwnerEmail.Trim()),
            PhoneNumber.Create(input.OwnerPhone.Trim()));
    }

    private static OrderShippingInfoRequestDto MapToOrderShippingInfoRequestDto(
        AdminCreateOrderShippingInfoRequest input)
    {
        return new OrderShippingInfoRequestDto(
            Enum.TryParse<ShippingMethod>(input.ShippingMethod.Trim(), out var shippingMethod)
                ? shippingMethod
                : throw new ArgumentException("Invalid shipping method"),
            input.ReceiverName.Trim(),
            PhoneNumber.Create(input.ReceiverPhone.Trim()),
            input.ShippingAddress.Trim(),
            string.Empty);
    }

    private static OrderItemRequestDto[] MapToOrderItemRequestDtos(
        IReadOnlyCollection<AdminCreateOrderItemRequestDto> items)
    {
        return [.. items.Select(i =>
            new OrderItemRequestDto(
                i.ProductId,
                i.VariationId,
                i.Quantity))];
    }
}
