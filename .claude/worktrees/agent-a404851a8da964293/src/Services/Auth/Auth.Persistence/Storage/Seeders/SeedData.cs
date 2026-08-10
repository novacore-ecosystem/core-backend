namespace NovaCore.Auth.Persistence.Storage.Seeders;

public static class SeedData
{
    public static class Roles
    {
        public const string Root = "Root";
        public const string Admin = "Admin";
        public const string User = "User";

        public static readonly List<(string Name, string? Description)> Default =
        [
            (Root, "Root administrator with unrestricted system access"),
            (Admin, "Administrator with full system management access"),
            (User, "Standard user role with basic permissions")
        ];
    }

    public static class Accounts
    {
        public const string RootUsername = "root";
        public const string RootEmail = "root@local.app";
        public const string RootPassword = "123456aA";

        public static readonly List<(string Username, string Email, string Password, string[] Roles)> Default =
        [
            (RootUsername, RootEmail, RootPassword, [Roles.Root])
        ];
    }
}
