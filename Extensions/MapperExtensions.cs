using AutoMapper;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Extensions
{
    public static class MapperExtensions
    {
        public static PagedResultDto<TDestination> MapPagedResult<TSource, TDestination>(
            this IMapper mapper,
            PagedResultDto<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);

            return source.ProjectTo(mapper.Map<TSource, TDestination>);
        }

        public static PagedResultDto<TDestination> MapPagedResult<TSource, TDestination>(
        this IMapper mapper,
        PagedResultDto<TSource> source,
        Action<IMappingOperationOptions<object, TDestination>> opts)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(opts);

            return source.ProjectTo(item => mapper.Map<TDestination>(item, opts));
        }
    }

}
