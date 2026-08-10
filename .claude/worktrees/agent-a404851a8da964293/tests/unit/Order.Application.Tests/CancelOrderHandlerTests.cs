using NSubstitute;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Orders.Commands.CancelOrder;
using NovaCore.Order.Domain.Entities;

using Shouldly;

namespace NovaCore.Order.Application.Tests;

/// <summary>
/// Regression coverage for B2: cancelling a Confirmed order (stock already deducted by
/// CreateOrderSaga's DeductInventoryStep) must restock, or the deduction leaks with no
/// corresponding active order. The Pending/Confirmed-only guard now lives inside
/// IOrderWriteService.CancelAsync (see the Course Correction in the persistence refactor tracker),
/// so the "invalid status" tests configure the substitute to throw the way the real
/// implementation would, rather than re-deriving that guard here.
/// </summary>
public sealed class CancelOrderHandlerTests
{
    private static OrderEntity CreateOrder() =>
        OrderEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Jane Doe", "0123456789", "123 Main St",
            [new OrderItemCreateModel(Guid.NewGuid(), "Widget", 10m, 1)]);

    private static (IOrderWriteService WriteService, IOutboxStore Outbox, IInventoryClientService Inventory, IUnitOfWork Uow)
        CreateSubstitutes(OrderEntity order)
    {
        var writeService = Substitute.For<IOrderWriteService>();
        writeService.CancelAsync(order.Id, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                order.Cancel(ci.ArgAt<string>(1));
                return Task.FromResult(order.Owner.OwnerId);
            });

        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });

        return (writeService, Substitute.For<IOutboxStore>(), Substitute.For<IInventoryClientService>(), uow);
    }

    [Theory]
    [InlineData(false)] // Pending
    [InlineData(true)]  // Confirmed - stock genuinely deducted by the saga
    public async Task Handle_RestocksInventory_KeyedByOrderId_AfterCancellationCommits(bool confirmed)
    {
        var order = CreateOrder();
        if (confirmed)
            order.Accept();

        var (writeService, outbox, inventory, uow) = CreateSubstitutes(order);
        var handler = new CancelOrderHandler(writeService, outbox, inventory, uow);

        await handler.Handle(new CancelOrderCommand(order.Id, "Changed my mind"));

        order.Status.ShouldBe(OrderStatus.Cancelled);
        await inventory.Received(1).RestockAsync(order.Id, Arg.Any<string?>(), Arg.Any<CancellationToken>());

        // Restock only happens once the cancellation is durably committed - not before, so a
        // rejected cancellation (invalid status) can never restock a still-active order.
        Received.InOrder(() =>
        {
            uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            inventory.RestockAsync(order.Id, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_DoesNotRestock_WhenOrderIsAlreadyCancelled()
    {
        var order = CreateOrder();
        order.Cancel("First cancellation");

        var (writeService, outbox, inventory, uow) = CreateSubstitutes(order);
        writeService.CancelAsync(order.Id, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Guid>(new BadRequestException("Order is already cancelled.")));

        var handler = new CancelOrderHandler(writeService, outbox, inventory, uow);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(new CancelOrderCommand(order.Id)));

        await inventory.DidNotReceive().RestockAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotRestock_WhenOrderIsCompleted()
    {
        var order = CreateOrder();
        order.Accept();
        order.Complete();

        var (writeService, outbox, inventory, uow) = CreateSubstitutes(order);
        writeService.CancelAsync(order.Id, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Guid>(new BadRequestException("Cannot cancel a completed order.")));

        var handler = new CancelOrderHandler(writeService, outbox, inventory, uow);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(new CancelOrderCommand(order.Id)));

        await inventory.DidNotReceive().RestockAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
