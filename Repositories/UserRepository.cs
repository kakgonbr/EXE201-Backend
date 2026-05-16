using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ExeContext _db;

        public UserRepository(ExeContext db)
        {
            _db = db;
        }

        public async Task<User?> GetById(int id)
        {
            return await _db.Users
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _db.Users
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task Add(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task<int> Update(User user)
        {
            _db.Users.Update(user);
            return await _db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var user = await GetById(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetAll()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<PagedResult<User>> GetAll(int page, int pageSize)
        {
            return await _db.Users.ToPagedResultAsync(page, pageSize);
        }

        public async Task<int> SaveAsync()
        {
            return await _db.SaveChangesAsync();
        }
    }
}
