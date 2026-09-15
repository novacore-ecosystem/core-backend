using NovaCore.Auth.Application.Abstractions.Persistence.LoginHistories;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Contexts.LoginHistories.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.LoginHistories.Write;

public sealed class LoginHistoryWriteService(ILoginHistoryRepository loginHistoryRepo)
    : ILoginHistoryWriteService, IPersistenceService
{
    public async Task RecordSuccessLoginAsync(
        Guid accountId,
        IpAddress ipAddress,
        CancellationToken ct = default)
    {
        var entry = LoginHistory.Record(accountId, ipAddress, LoginResult.Success);
        await loginHistoryRepo.AddAsync(entry, ct);
    }
}
