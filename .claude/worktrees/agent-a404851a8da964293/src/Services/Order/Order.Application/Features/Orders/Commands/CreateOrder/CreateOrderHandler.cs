using NovaCore.BuildingBlock.Contract.Events.Order;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Orders.DTOs;

namespace NovaCore.Order.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    ICurrentUserService currentUser,
    IOrderItemPreparationService itemPreparationService,
    IOrderReadService orderReadService,
    IOrderWriteService orderWriteService,
    IUnitOfWork uow,
    IOutboxStore outboxStore) : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        // Validate idempotency key and check for existing order with the same key
        var idempotencyId = currentUser.GetIdempotencyKey()
            ?? throw new BadRequestException(MessageCode.InvalidInput, "Missing currelation ID from Header.");
        await CheckExistingIdempotentOrderAsync(idempotencyId, ct);

        // Check if order items match the user's cart (if not admin-created), resolve catalog
        // pricing, and ensure stock availability
        var isAdminCreated = request.CreatedById.HasValue;
        var preparedItems = await itemPreparationService.PrepareAsync(
            isAdminCreated,
            request.Owner.OwnerId,
            request.Items,
            ct);

        var response = await CreateAndPublishAsync(
            idempotencyId,
            request.Owner,
            request.ShippingInfo,
            preparedItems,
            request.CreatedById,
            ct);

        // Clear the user's cart if the order was created by the user (not an admin)
        // if (!request.CreatedById.HasValue)
        //    await cartService.ClearCartAsync(request.Owner.OwnerId, ct);

        return response;
    }

    #region Check existing idempotent order
    private async Task CheckExistingIdempotentOrderAsync(string idempotencyKey, CancellationToken ct)
    {
        var duplicate = await orderReadService.GetByIdempotencyKeyAsync(idempotencyKey, ct);
        if (duplicate is not null)
            throw new ConflictException(
                $"OrderId: {duplicate.Id} - Order Number: {duplicate.OrderNumber}. " +
                "An order with the same idempotency key already exists. " +
                "If you are retrying a previous request, please use the same idempotency key.");
    }
    #endregion

    #region Create order and publish event bus
    private async Task<CreateOrderResponse> CreateAndPublishAsync(
        string idempotencyId,
        OrderOwnerRequestDto owner,
        OrderShippingInfoRequestDto shippingInfo,
        IReadOnlyList<PreparedOrderItem> preparedItems,
        Guid? createdById,
        CancellationToken ct)
    {
        var correlationId = currentUser.GetCorrelationId();

        var createOrderRequest = new CreateOrderRequest(
            idempotencyId,
            createdById,
            owner,
            shippingInfo,
            preparedItems);

        OrderEntity order = null!;

        // Persist data and create outbox
        await uow.ExecuteTransactionAsync(async () =>
        {
            order = await orderWriteService.CreateAsync(createOrderRequest, ct);

            var orderCreatedItems = order.Items
                .Select(i => new OrderCreatedItem(
                    i.ProductId,
                    i.VariationId,
                    i.ProductName,
                    i.Quantity.Value,
                    i.UnitPrice.Value))
                .ToArray();

            var eventBus = new OrderCreatedIntegrationEvent(
                order.TenantId,
                order.Id,
                owner.OwnerId,
                orderCreatedItems,
                order.GrandTotal.Value,
                correlationId);
            await outboxStore.EnqueueAsync(eventBus, ct);
        }, ct: ct);

        // TODO: Dispatch event directly
        // ...
        // ...
        // ...

        return new CreateOrderResponse(
            order.Id,
            order.OrderNumber.Value);
    }
    #endregion
}
