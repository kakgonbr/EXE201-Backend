using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public enum HostWithdrawSort
    {
        Id = 0,
        UserName,
        Amount,
        Status
    }

    public enum WithdrawStatusFilter
    {
        All = 0,
        Pending,
        Approved,
        Rejected
    }

    public class HostWithdrawRepository : IHostWithdrawRepository
    {
        private readonly ExeContext _db;

        public HostWithdrawRepository(ExeContext db)
        {
            _db = db;
        }

        public async Task<HostWithdraw?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.HostWithdraws
                .Include(hw => hw.User)
                .SingleOrDefaultAsync(hw => hw.Id == id, cancellationToken);
        }

        public async Task AddAsync(HostWithdraw hostWithdraw, CancellationToken cancellationToken = default)
        {
            await _db.HostWithdraws.AddAsync(hostWithdraw, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(HostWithdraw hostWithdraw, CancellationToken cancellationToken = default)
        {
            _db.HostWithdraws.Update(hostWithdraw);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var hostWithdraw = await GetByIdAsync(id, cancellationToken);
            if (hostWithdraw != null)
            {
                _db.HostWithdraws.Remove(hostWithdraw);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<HostWithdraw>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.HostWithdraws.ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<HostWithdraw>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await GetAllAsync(null, null, false, page, pageSize, cancellationToken);
        }

        public async Task<PagedResultDto<HostWithdraw>> GetAllAsync(
            WithdrawStatusFilter? statusFilter = null,
            HostWithdrawSort? sortBy = null,
            bool sortDesc = false,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _db.HostWithdraws
                .AsNoTracking()
                .Include(hw => hw.User)
                .AsQueryable();

            if (statusFilter.HasValue && statusFilter.Value != WithdrawStatusFilter.All)
            {
                switch (statusFilter.Value)
                {
                    case WithdrawStatusFilter.Approved:
                        query = query.Where(hw => hw.Status == "approved");
                        break;
                    case WithdrawStatusFilter.Pending:
                        query = query.Where(hw => hw.Status == "pending");
                        break;
                    case WithdrawStatusFilter.Rejected:
                        query = query.Where(hw => hw.Status == "rejected");
                        break;
                }
            }

            sortBy ??= HostWithdrawSort.Id;

            switch (sortBy.Value)
            {
                case HostWithdrawSort.UserName:
                    query = sortDesc
                        ? query.OrderByDescending(hw => hw.User != null ? hw.User.Name : string.Empty)
                        : query.OrderBy(hw => hw.User != null ? hw.User.Name : string.Empty);
                    break;

                case HostWithdrawSort.Amount:
                    query = sortDesc
                        ? query.OrderByDescending(hw => hw.Amount)
                        : query.OrderBy(hw => hw.Amount);
                    break;

                case HostWithdrawSort.Status:
                    query = sortDesc
                        ? query.OrderByDescending(hw => hw.Status)
                        : query.OrderBy(hw => hw.Status);
                    break;

                case HostWithdrawSort.Id:
                default:
                    query = sortDesc ? query.OrderByDescending(hw => hw.Id) : query.OrderBy(hw => hw.Id);
                    break;
            }

            return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalWithdrawnByHostIdAsync(int hostId, CancellationToken cancellationToken = default)
        {
            return await _db.HostWithdraws
                .Where(hw => hw.UserId == hostId && hw.Status == "approved")
                .SumAsync(hw => hw.Amount, cancellationToken);
        }
    }
}