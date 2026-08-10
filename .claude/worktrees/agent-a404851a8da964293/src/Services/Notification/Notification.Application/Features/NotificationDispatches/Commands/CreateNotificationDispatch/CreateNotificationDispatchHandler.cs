using NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;

namespace NovaCore.Notification.Application.Features.NotificationDispatches.Commands.CreateNotificationDispatch;

public sealed class CreateNotificationDispatchHandler(
    IUnitOfWork uow,
    INotificationDispatchWriteService dispatchWriteService) : ICommandHandler<CreateNotificationDispatchCommand>
{
    public async Task Handle(CreateNotificationDispatchCommand request, CancellationToken ct = default)
    {
        foreach (var channel in request.Types)
        {
            var dispatch = NotificationDispatch.Create(
                Guid.CreateVersion7(),
                request.Reference,
                channel,
                request.Payload,
                request.TemplateId);

            await dispatchWriteService.CreateAsync(dispatch, ct);
        }

        await uow.SaveChangesAsync(ct);
    }
}
