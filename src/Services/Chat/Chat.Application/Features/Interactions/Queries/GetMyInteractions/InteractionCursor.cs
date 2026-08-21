using System.Text;

namespace NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;

/// <summary>
/// Opaque cursor over OccurredAt alone (not a full keyset tuple like QueueCursor) - this feed
/// merge-sorts two independent tables (mentions, reactions) in the handler rather than a single
/// ordered source, so a single-column "strictly before this timestamp" cursor is what keeps the
/// merge simple. Trade-off: two items from different tables landing on the exact same DateTime
/// tick could theoretically both fall on the same side of a page boundary - acceptable for a
/// lazy-loaded notification feed, not the kind of gapless guarantee message sequencing needs.
/// </summary>
public static class InteractionCursor
{
    public static string Encode(DateTime occurredAt) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(occurredAt.Ticks.ToString()));

    public static bool TryDecode(string? cursor, out long beforeTicks)
    {
        beforeTicks = 0;

        if (string.IsNullOrWhiteSpace(cursor))
            return false;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return long.TryParse(decoded, out beforeTicks);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
