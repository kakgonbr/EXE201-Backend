using EXE201_Backend.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> source,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

            var totalCount = await source.LongCountAsync(cancellationToken);

            if (totalCount == 0)
                return PagedResult<T>.Empty(page, pageSize);

            var data = await source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return PagedResult<T>.Create(data, page, pageSize, totalCount);
        }
    }

}
