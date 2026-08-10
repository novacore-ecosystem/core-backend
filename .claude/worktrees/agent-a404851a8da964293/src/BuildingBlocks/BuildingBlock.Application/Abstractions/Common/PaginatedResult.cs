namespace NovaCore.BuildingBlock.Application.Abstractions.Common;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PaginatedResult<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
