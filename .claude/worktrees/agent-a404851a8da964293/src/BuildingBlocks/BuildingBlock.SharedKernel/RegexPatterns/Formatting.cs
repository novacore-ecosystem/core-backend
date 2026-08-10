using System.Text.RegularExpressions;

namespace NovaCore.BuildingBlock.SharedKernel.RegexPatterns;

public static partial class RegexPatterns
{
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 500)]
    public static partial Regex Slug();

    [GeneratedRegex(@"^[A-Z1-9](?!.*--)[A-Z0-9-]{1,28}[A-Z0-9]$", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 500)]
    public static partial Regex Sku();

    [GeneratedRegex("^[0-9]{8,14}$", RegexOptions.None, matchTimeoutMilliseconds: 500)]
    public static partial Regex BarcodeFormat();
}
