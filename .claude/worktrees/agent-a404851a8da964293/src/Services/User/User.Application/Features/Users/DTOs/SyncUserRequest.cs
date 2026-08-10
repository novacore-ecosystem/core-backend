namespace NovaCore.User.Application.Features.Users.DTOs;

/// <summary>Used by OnUserInitiated to mirror an Account already created in Auth - AccountId becomes this User's id.</summary>
public sealed record SyncUserRequest(
    Guid AccountId,
    string Username,
    string Email,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName);
