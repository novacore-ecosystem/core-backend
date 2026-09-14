using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.AssignAccountApp;

public sealed class AssignAccountAppHandler(
    IUnitOfWork unitOfWork,
    IAccountAppAssignmentService accountAppAssignmentService,
    IAppMembershipCache appMembershipCache) : ICommandHandler<AssignAccountAppCommand>
{
    public async Task Handle(AssignAccountAppCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountAppAssignmentService.AssignAsync(request.AccountId, request.AppId, ct);
        }, ct: ct);

        // Invalidate only after the transaction commits - the next membership lookup rebuilds
        // a fresh set from the database.
        await appMembershipCache.InvalidateAsync(request.AppId, ct);
    }
}
