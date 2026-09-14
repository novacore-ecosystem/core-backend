using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Auth.Application.Features.Apps.Commands.CreateApp;

public sealed class CreateAppHandler(
    IUnitOfWork unitOfWork,
    IAppReadService appReadService,
    IAppWriteService appWriteService,
    IAppCollectionCache appCollectionCache) : ICommandHandler<CreateAppCommand, Guid>
{
    public async Task<Guid> Handle(CreateAppCommand request, CancellationToken ct = default)
    {
        var code = AppCode.Create(request.Code);

        if (await appReadService.ExistsByCodeAsync(code, ct))
            throw new ConflictException($"App with code ({code.Value}) already exists.");

        App newApp = null!;
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            newApp = App.Create(code, request.Name);
            await appWriteService.CreateAsync(newApp, ct);
        }, ct: ct);

        // Invalidate only after the transaction commits - the next App read rebuilds the
        // collection cache from the database.
        await appCollectionCache.InvalidateAsync(ct);

        return newApp.Id;
    }
}
