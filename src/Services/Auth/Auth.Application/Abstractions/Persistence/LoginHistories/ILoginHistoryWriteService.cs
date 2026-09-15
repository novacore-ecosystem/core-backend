using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Abstractions.Persistence.LoginHistories;

public interface ILoginHistoryWriteService
{
    Task RecordSuccessLoginAsync(
        Guid accountId,
        IpAddress ipAddress,
        CancellationToken ct = default);
}
