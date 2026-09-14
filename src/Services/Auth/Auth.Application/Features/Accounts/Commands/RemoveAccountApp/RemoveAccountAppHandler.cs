using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.RemoveAccountApp;

public sealed class RemoveAccountAppHandler(
    IUnitOfWork unitOfWork,
    IAccountAppAssignmentService accountAppAssignmentService) : ICommandHandler<RemoveAccountAppCommand>
{
    public async Task Handle(RemoveAccountAppCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountAppAssignmentService.RemoveAsync(request.AccountId, request.AppId, ct);
        }, ct: ct);
    }
}
