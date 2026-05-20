using EXE201_Backend.Models.Dto;
using EXE201_Backend.Models.Responses;

namespace EXE201_Backend.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUser(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<UserDto>> GetUsers(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}