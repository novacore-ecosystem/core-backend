using NovaCore.Order.Application.Abstractions.Persistence.Orders;

namespace NovaCore.Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed class UpdateOrderOwnerInfoHandler(
    ICurrentUserService currentUser,
    IOrderWriteService orderWriteService,
    IUnitOfWork uow) : ICommandHandler<UpdateOrderOwnerInfoCommand>
{
    public async Task Handle(UpdateOrderOwnerInfoCommand request, CancellationToken ct = default)
    {
        var idempotencyKey = currentUser.GetIdempotencyKey()
            ?? throw new BadRequestException(MessageCode.InvalidInput, "Missing currelation ID from Header.");

        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderWriteService.UpdateOwnerInfoAsync(
                request.OrderId,
                request.OwnerName,
                request.OwnerEmail,
                request.OwnerPhone,
                idempotencyKey,
                ct);
        }, ct: ct);
    }
}
