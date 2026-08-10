using System.Text.RegularExpressions;

namespace NovaCore.Order.Domain.Regexes;

public static partial class CommonRegexes
{
    [GeneratedRegex(@"^ORD-\d{8}-\d{4}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    public static partial Regex OrderNumber();
}
