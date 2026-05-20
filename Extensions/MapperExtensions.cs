using AutoMapper;
using EXE201_Backend.Models.Responses;

namespace EXE201_Backend.Extensions
{
    public static class MapperExtensions
    {
        public static PagedResult<TDestination> MapPagedResult<TSource, TDestination>(
            this IMapper mapper,
            PagedResult<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);

            return source.ProjectTo(mapper.Map<TSource, TDestination>);
        }
    }

}
