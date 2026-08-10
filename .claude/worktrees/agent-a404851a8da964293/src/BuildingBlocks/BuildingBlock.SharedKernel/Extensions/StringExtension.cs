namespace NovaCore.BuildingBlock.SharedKernel.Extensions;

public static class StringExtension
{
    public static string ToStringOrEmpty(this string? input)
        => input?.ToString() ?? string.Empty;

    public static bool IsNullOrWhiteSpace(this string? input)
        => string.IsNullOrWhiteSpace(input);

    public static bool IsNotNullOrWhiteSpace(this string? input)
        => !string.IsNullOrWhiteSpace(input);

    public static string GetFileName<T>(this T _) => typeof(T).Name;

    public static string GetUpperName<T>(this T _) => typeof(T).Name.ToUpperInvariant();

    public static string GetLowerName<T>(this T _) => typeof(T).Name.ToLowerInvariant();
}
