using System.Text.RegularExpressions;

namespace NovaCore.Shipping.Domain.Regexes;

public static partial class ShippingRegexes
{
    [GeneratedRegex(@"^SHP-\d{8}-[A-Z0-9]{4}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    public static partial Regex ShipmentNumber();

    [GeneratedRegex(@"^TRN-\d{8}-[A-Z0-9]{4}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    public static partial Regex TransportationNumber();
}
