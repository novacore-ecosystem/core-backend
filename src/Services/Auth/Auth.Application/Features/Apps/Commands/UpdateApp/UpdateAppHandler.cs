using NovaCore.Auth.Application.Abstractions.Persistence.Apps;

namespace NovaCore.Auth.Application.Features.Apps.Commands.UpdateApp;

public sealed class UpdateAppHandler(
    IUnitOfWork unitOfWork,
    IAppWriteService appWriteService) : ICommandHandler<UpdateAppCommand>
{
    public async Task Handle(UpdateAppCommand request, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await appWriteService.UpdateAsync(request.Id, app =>
            {
                app.Rename(request.Name);

                if (request.IsActive)
                    app.Activate();
                else
                    app.Deactivate();
            }, ct);
        }, ct: ct);
    }
}
