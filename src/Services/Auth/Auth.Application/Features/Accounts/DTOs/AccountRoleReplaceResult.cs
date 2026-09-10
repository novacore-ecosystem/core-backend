namespace NovaCore.Auth.Application.Features.Accounts.DTOs;

public sealed record AccountRoleReplaceResult(bool HasChanges, IReadOnlySet<Guid> ResultingRoleIds);
