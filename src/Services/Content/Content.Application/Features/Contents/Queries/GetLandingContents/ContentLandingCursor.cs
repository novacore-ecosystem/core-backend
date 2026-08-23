using System.Globalization;
using System.Text;

namespace NovaCore.Content.Application.Features.Contents.Queries.GetLandingContents;

/// <summary>Opaque pagination cursor for the Landing content list - base64 of
/// "{PublishedAt:O}|{Id}", matching the fixed PublishedAt-desc/Id-desc keyset
/// ContentReadService.SearchLandingAsync queries by. Shared by GetLandingContents and
/// GetLandingContentCursorPoints, which use identical filter/pagination semantics.</summary>
internal static class ContentLandingCursor
{
    public static string Encode(DateTime publishedAt, Guid id) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{publishedAt:O}|{id}"));

    public static (DateTime PublishedAt, Guid Id)? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|', 2);
            if (parts.Length != 2)
                return null;

            var publishedAt = DateTime.Parse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var id = Guid.Parse(parts[1]);
            return (publishedAt, id);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
