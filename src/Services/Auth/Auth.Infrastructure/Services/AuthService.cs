using NovaCore.Auth.Application.Abstractions.Auth;

using NovaCore.BuildingBlock.Domain.ValueObjects;

using Microsoft.AspNetCore.Identity;

namespace NovaCore.Auth.Infrastructure.Services;

public sealed class AuthService(UserManager<Account> userManager) : IAppService, IAuthService
{
    private readonly UserManager<Account> _userManager = userManager;

    public async Task<Account?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<Account?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return false;

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> ValidateCredentialsAsync(Account account, string password, CancellationToken ct = default)
        => await _userManager.CheckPasswordAsync(account, password);

    public Task<Account?> CreateUserAsync(
        string email,
        string username,
        string password,
        CancellationToken ct = default)
        => CreateUserInternalAsync(Account.Create(username, Email.Create(email)), password);

    public Task<Account?> CreateUserAsync(
        Guid id,
        string email,
        string username,
        string password,
        CancellationToken ct = default)
        => CreateUserInternalAsync(Account.Create(id, username, Email.Create(email)), password);

    private async Task<Account?> CreateUserInternalAsync(Account user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded ? user : null;
    }

    public async Task<bool> UpdatePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> SetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        if (await _userManager.HasPasswordAsync(user))
        {
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                return false;
        }

        var addResult = await _userManager.AddPasswordAsync(user, newPassword);
        return addResult.Succeeded;
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        user.ConfirmEmail();
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}
