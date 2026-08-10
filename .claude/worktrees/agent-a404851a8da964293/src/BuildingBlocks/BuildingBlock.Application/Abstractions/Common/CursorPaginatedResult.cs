namespace NovaCore.BuildingBlock.Application.Abstractions.Common;

/// <summary>Cursor/lazy-load sibling of <see cref="PaginatedResult{T}"/> - no page number or total count, just the next opaque cursor to pass back in (null when there's nothing more).</summary>
public class CursorPaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public string? NextCursor { get; set; }
    public bool HasMore => NextCursor is not null;

    public static CursorPaginatedResult<T> Create(IReadOnlyList<T> items, string? nextCursor)
    {
        return new CursorPaginatedResult<T>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }
}
