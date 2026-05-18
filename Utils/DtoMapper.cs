using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Utils
{
    public class DtoMapper : AutoMapper.Profile
    {
        public DtoMapper()
        {
            CreateMap<Models.User, UserDto>();
            CreateMap<Models.Workshop, WorkshopDisplayDto>();
        }
    }
}
