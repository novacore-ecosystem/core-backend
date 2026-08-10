namespace NovaCore.BuildingBlock.SharedKernel.Utilities;

public static class NativeBarcodeGenerator
{
    private static readonly Random _random = new();

    public static string GenerateEan13()
    {
        string prefix = "2";

        var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long elapsedTicks = DateTime.UtcNow.Ticks - epoch.Ticks;
        long elapsedMilliseconds = elapsedTicks / TimeSpan.TicksPerMillisecond;

        string timePart = (elapsedMilliseconds % 1000000000).ToString("D9");

        string randomPart = _random.Next(0, 100).ToString("D2");

        string twelveDigits = $"{prefix}{timePart}{randomPart}";

        int checksum = CalculateEan13Checksum(twelveDigits);

        return $"{twelveDigits}{checksum}";
    }

    private static int CalculateEan13Checksum(string twelveDigits)
    {
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = twelveDigits[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }
        int remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }
}
