using System.Text.RegularExpressions;

namespace NovaCore.User.Application.Common.Regex;

/// <summary>Shared phone-number format check, reused by CreateUserValidator/UpdateUserValidator so the rule can't silently diverge between them.</summary>
internal static partial class PhoneNumberPattern
{
    private const string Format = @"^\d{10,}$";

    [GeneratedRegex(Format)]
    public static partial System.Text.RegularExpressions.Regex Value();
}
