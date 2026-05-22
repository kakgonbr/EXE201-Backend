using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Services
{
    public interface IUserService
    {
        Task<bool> ChangeAvatar(int userId, string newAvatarUrl, CancellationToken cancellationToken = default);
        Task<bool> ChangePhonenumber(int userId, string newPhoneNumber, CancellationToken cancellationToken = default);
        Task<bool> ChangeUsername(int userId, string newName, CancellationToken cancellationToken = default);
        Task<UserDto> GetUser(int id, CancellationToken cancellationToken = default);
        Task<PagedResultDto<UserDto>> GetUsers(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}