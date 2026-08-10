namespace NovaCore.BuildingBlock.SharedKernel.Text;

public static class PhoneNormalizer
{
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return new string(phone.Where(char.IsDigit).ToArray());
    }

    public static string Reverse(string normalized)
    {
        var chars = normalized.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}
