namespace NovaCore.BuildingBlock.SharedKernel.Extensions;

public static class ArrayExtension
{
    public static string JoinToString(this IEnumerable<string> list, string separate = "")
        => string.Join(separate, list);
}