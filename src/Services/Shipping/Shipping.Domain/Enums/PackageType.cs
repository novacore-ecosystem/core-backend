namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Physical container a Package is shipped in - drives dimensional pricing and handling rules.</summary>
public enum PackageType
{
    Box = 1,
    Envelope = 2,
    Bag = 3,
    Pallet = 4,
    Crate = 5,
    Other = 99,
}
