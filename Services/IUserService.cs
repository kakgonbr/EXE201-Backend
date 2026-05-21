using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUser(int id, CancellationToken cancellationToken = default);
        Task<PagedResultDto<UserDto>> GetUsers(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}