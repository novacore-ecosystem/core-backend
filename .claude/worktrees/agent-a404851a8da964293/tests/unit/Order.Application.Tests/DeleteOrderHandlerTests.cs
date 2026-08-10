using NSubstitute;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Orders.Commands.DeleteOrder;
using NovaCore.Order.Domain.Entities;

using Shouldly;

namespace NovaCore.Order.Application.Tests;

/// <summary>
/// Regression coverage for B2's narrower variant: a Pending order can already have had stock
/// deducted (CreateOrderSaga's DeductInventoryStep runs before ConfirmOrderStep), so hard-deleting
/// it must still restock first, or the deduction leaks with no order left to point at.
/// </summary>
public sealed class DeleteOrderHandlerTests
{
    private static OrderEntity CreateOrder() =>
        OrderEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "0123456789", "123 Main St",
            [new OrderItemCreateModel(Guid.NewGuid(), "Widget", 10m, 1)]);

    private static (IOrderReadService ReadService, IOrderWriteService WriteService, IOutboxStore Outbox, IInventoryClientService Inventory, IUnitOfWork Uow)
        CreateSubstitutes(OrderEntity order)
    {
        var readService = Substitute.For<IOrderReadService>();
        readService.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var writeService = Substitute.For<IOrderWriteService>();

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        return (readService, writeService, Substitute.For<IOutboxStore>(), Substitute.For<IInventoryClientService>(), uow);
    }

    [Theory]
    [InlineData(false)] // Pending - may already be mid-saga with stock deducted
    [InlineData(true)]  // Cancelled - idempotent no-op if CancelOrderHandler already restocked
    public async Task Handle_RestocksInventory_BeforeDeleting(bool cancelled)
    {
        var order = CreateOrder();
        if (cancelled)
            order.Cancel("No longer needed");

        var (readService, writeService, outbox, inventory, uow) = CreateSubstitutes(order);
        var handler = new DeleteOrderHandler(readService, writeService, outbox, inventory, uow);

        await handler.Handle(new DeleteOrderCommand(order.Id));

        await inventory.Received(1).RestockAsync(order.Id, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await writeService.Received(1).DeleteAsync(order.Id, Arg.Any<CancellationToken>());

        // Restock happens before the delete, not after - if restock fails, the order must survive
        // so a retry has something to restock against.
        Received.InOrder(() =>
        {
            inventory.RestockAsync(order.Id, Arg.Any<string?>(), Arg.Any<CancellationToken>());
            writeService.DeleteAsync(order.Id, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_DoesNotRestockOrDelete_WhenOrderIsConfirmed()
    {
        var order = CreateOrder();
        order.Accept();

        var (readService, writeService, outbox, inventory, uow) = CreateSubstitutes(order);
        var handler = new DeleteOrderHandler(readService, writeService, outbox, inventory, uow);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(new DeleteOrderCommand(order.Id)));

        await inventory.DidNotReceive().RestockAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await writeService.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
