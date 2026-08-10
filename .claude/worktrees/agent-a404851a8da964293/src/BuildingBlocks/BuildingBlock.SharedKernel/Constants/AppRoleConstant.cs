namespace NovaCore.BuildingBlock.SharedKernel.Constants;

public static class AppRoleConstant
{
    /// <summary>
    /// Superuser role. Satisfies every role check and authorization policy.
    /// </summary>
    public const string Root = "Root";
    public const string Admin = "Admin";
    public const string User = "User";

    public static readonly string[] SupportedRoles = [Root, Admin, User];
}
