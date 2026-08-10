namespace NovaCore.User.Application.Features.Users.DTOs;

/// <summary>
/// Used by (admin-invoked) CreateUser. Role/Position assignment is deliberately absent here -
/// that concept belongs to Auth's own Account/Role/Position aggregates (see
/// docs/services/user-service.md), not User's local UserRole segmentation bundles, and the
/// cross-service admin-provisioning workflow that will supply them is future work.
/// </summary>
public sealed record CreateUserRequest(
    string Username,
    string DisplayName,
    UserType UserType,
    string Email,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName);
