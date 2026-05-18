using EXE201_Backend.Models;

namespace EXE201_Backend.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task DeleteAsync(int id);
        Task<List<User>> GetAllAsync();
        Task<PagedResult<User>> GetAllAsync(int page, int pageSize);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<int> SaveAsync();
        Task<int> UpdateAsync(User user);
    }
}