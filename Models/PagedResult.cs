namespace EXE201_Backend.Models
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Data { get; init; } = [];

        public int Page { get; init; }

        public int PageSize { get; init; }

        public long TotalCount { get; init; }

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;

        public static PagedResult<T> Create(IEnumerable<T> data, int page, int pageSize, long totalCount)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

            return new PagedResult<T>
            {
                Data = data.ToList().AsReadOnly(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }

        public PagedResult<TDestination> ProjectTo<TDestination>(Func<T, TDestination> selector)
        {
            ArgumentNullException.ThrowIfNull(selector);

            return new PagedResult<TDestination>
            {
                Data = Data.Select(selector).ToList().AsReadOnly(),
                Page = Page,
                PageSize = PageSize,
                TotalCount = TotalCount,
            };
        }

        public static PagedResult<T> Empty(int page = 1, int pageSize = 10) =>
            Create([], page, pageSize, totalCount: 0);
    }

}
