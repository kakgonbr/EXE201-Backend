using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUser(int id);
        Task<PagedResult<UserDto>> GetUsers(int page, int pageSize);
    }
}