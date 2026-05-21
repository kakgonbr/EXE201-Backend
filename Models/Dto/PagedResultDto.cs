namespace EXE201_Backend.Models.Dto
{
    public class PagedResultDto<T>
    {
        public IReadOnlyList<T> Data { get; init; } = [];

        public int Page { get; init; }

        public int PageSize { get; init; }

        public long TotalCount { get; init; }

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;

        public static PagedResultDto<T> Create(IEnumerable<T> data, int page, int pageSize, long totalCount)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

            return new PagedResultDto<T>
            {
                Data = data.ToList().AsReadOnly(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }

        public PagedResultDto<TDestination> ProjectTo<TDestination>(Func<T, TDestination> selector)
        {
            ArgumentNullException.ThrowIfNull(selector);

            return new PagedResultDto<TDestination>
            {
                Data = Data.Select(selector).ToList().AsReadOnly(),
                Page = Page,
                PageSize = PageSize,
                TotalCount = TotalCount,
            };
        }

        public static PagedResultDto<T> Empty(int page = 1, int pageSize = 10) =>
            Create([], page, pageSize, totalCount: 0);
    }

}
