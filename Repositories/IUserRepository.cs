using EXE201_Backend.Models;

namespace EXE201_Backend.Repositories
{
    public interface IUserRepository
    {
        Task Add(User user);
        Task Delete(int id);
        Task<List<User>> GetAll();
        Task<PagedResult<User>> GetAll(int page, int pageSize);
        Task<User?> GetByEmail(string email);
        Task<User?> GetById(int id);
        Task<int> SaveAsync();
        Task<int> Update(User user);
    }
}