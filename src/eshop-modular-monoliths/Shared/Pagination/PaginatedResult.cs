namespace Shared.Pagination;

public class PaginationResult<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
    where TEntity : class
{
    private int PageIndex { get; } = pageIndex;
    private int PageSize { get; } = pageSize;
    public long Count { get; } = count;
    public IEnumerable<TEntity> Data { get; } = data;

}