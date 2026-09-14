using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.AssignAccountApp;

public sealed class AssignAccountAppHandler(
    IUnitOfWork unitOfWork,
    IAccountAppAssignmentService accountAppAssignmentService) : ICommandHandler<AssignAccountAppCommand>
{
    public async Task Handle(AssignAccountAppCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountAppAssignmentService.AssignAsync(request.AccountId, request.AppId, ct);
        }, ct: ct);
    }
}
