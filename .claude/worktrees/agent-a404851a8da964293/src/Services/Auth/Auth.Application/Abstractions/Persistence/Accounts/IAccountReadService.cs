namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

public interface IAccountReadService
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);
}
