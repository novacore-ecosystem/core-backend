namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

public interface IAccountWriteService
{
    Task DeleteIfExistAsync(Guid id, CancellationToken ct = default);
}
