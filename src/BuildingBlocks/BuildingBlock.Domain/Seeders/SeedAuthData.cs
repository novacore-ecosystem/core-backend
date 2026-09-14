namespace NovaCore.BuildingBlock.Domain.Seeders;

public static class SeedAuthData
{
    public static class Accounts
    {
        public static readonly Guid RootId = new("019f5a81-ef94-76d5-af63-6204c51b6c62");
        public const string RootUsername = "root";
        public const string RootEmail = "root@novacore.local";
        public const string RootPassword = "Root@1234";

        public static readonly List<(Guid Id, string Username, string Email, string Password)> Default =
        [
            (RootId, RootUsername, RootEmail, RootPassword)
        ];
    }
}
