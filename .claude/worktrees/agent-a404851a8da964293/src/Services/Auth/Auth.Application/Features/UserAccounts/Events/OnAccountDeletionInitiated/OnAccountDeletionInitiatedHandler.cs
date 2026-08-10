namespace NovaCore.Auth.Application.Features.UserAccounts.Events.OnAccountDeletionInitiated;

public sealed class OnAccountDeletionInitiatedHandler(
    IAccountWriteService accountWriteService,
    IUnitOfWork unitOfWork
) : IInternalEventHandler<OnAccountDeletionInitiatedEvent>
{
    public async Task Handle(OnAccountDeletionInitiatedEvent @event, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountWriteService.DeleteIfExistAsync(@event.AccountId, ct);
        }, ct: ct);
    }
}