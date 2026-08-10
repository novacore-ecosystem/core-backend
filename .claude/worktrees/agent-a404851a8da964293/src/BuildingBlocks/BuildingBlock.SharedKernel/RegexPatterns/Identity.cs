using System.Text.RegularExpressions;

namespace NovaCore.BuildingBlock.SharedKernel.RegexPatterns;

public static partial class RegexPatterns
{
    [GeneratedRegex(@"^((?!\.)[\w\-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex Email();

    [GeneratedRegex(@"/(?:([+]\d{1,4})[-.\s]?)?(?:[(](\d{1,3})[)][-.\s]?)?(\d{1,4})[-.\s]?(\d{1,4})[-.\s]?(\d{1,9})/g", RegexOptions.Compiled)]
    public static partial Regex PhoneNumber();
}
