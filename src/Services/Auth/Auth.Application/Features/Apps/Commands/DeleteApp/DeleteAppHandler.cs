using NovaCore.Auth.Application.Abstractions.Persistence.Apps;

namespace NovaCore.Auth.Application.Features.Apps.Commands.DeleteApp;

public sealed class DeleteAppHandler(
    IUnitOfWork unitOfWork,
    IAppWriteService appWriteService) : ICommandHandler<DeleteAppCommand>
{
    public async Task Handle(DeleteAppCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await appWriteService.DeleteAsync(request.Id, ct);
        }, ct: ct);
    }
}
