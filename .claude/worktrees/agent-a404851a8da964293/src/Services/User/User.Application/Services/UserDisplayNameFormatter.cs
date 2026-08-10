using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Services;

public sealed class UserDisplayNameFormatter : IUserDisplayNameFormatter
{
    public string Format(string firstName, string middleName, string lastName, string locale)
    {
        string[] parts = IsVietnamese(locale)
            ? [lastName, middleName, firstName]
            : [firstName, middleName, lastName];

        return string.Join(' ', parts
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim()));
    }

    private static bool IsVietnamese(string locale) =>
        locale.StartsWith("vi", StringComparison.OrdinalIgnoreCase);
}
