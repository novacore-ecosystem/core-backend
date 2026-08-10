namespace NovaCore.TestKit.Random;

/// <summary>
/// Collision-free identifiers for test data, so two tests running in parallel (or two entities
/// in the same test) never accidentally share a magic-literal id/code.
/// </summary>
public static class TestId
{
    public static Guid NewGuid() => Guid.CreateVersion7();

    /// <summary>Short unique suffix (8 hex chars) for building readable-but-unique codes, e.g. $"SKU-{TestId.Suffix()}".</summary>
    public static string Suffix() => Guid.CreateVersion7().ToString("N")[..8];
}
