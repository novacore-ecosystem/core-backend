namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

public interface IAccountWriteService
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);

    Task SetLevelAsync(Guid id, int level, CancellationToken ct = default);
}
