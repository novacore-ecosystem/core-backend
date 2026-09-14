using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;

namespace NovaCore.Auth.Application.Features.Apps.Commands.DeleteApp;

public sealed class DeleteAppHandler(
    IUnitOfWork unitOfWork,
    IAppWriteService appWriteService,
    IAppCollectionCache appCollectionCache) : ICommandHandler<DeleteAppCommand>
{
    public async Task Handle(DeleteAppCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await appWriteService.DeleteAsync(request.Id, ct);
        }, ct: ct);

        // Invalidate only after the transaction commits - the next App read rebuilds the
        // collection cache from the database.
        await appCollectionCache.InvalidateAsync(ct);
    }
}
