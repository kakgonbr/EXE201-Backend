using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public enum HostRegistrationSort
    {
        Id = 0,
        UserName,
        Approved
    }

    public enum ApproveStatusFilter
    {
        Both = 0,
        NotApproved,
        Approved
    }

    public class HostRegistrationRepository : IHostRegistrationRepository
    {
        private readonly ExeContext _db;
        public HostRegistrationRepository(ExeContext db)
        {
            _db = db;
        }

        public async Task<HostRegistration?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.HostRegistrations
                .Include(hr => hr.User)
                .Include(hr => hr.ApprovedByNavigation)
                .SingleOrDefaultAsync(hr => hr.Id == id, cancellationToken);
        }

        public async Task<HostRegistration?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _db.HostRegistrations
                .Include(hr => hr.User)
                .Include(hr => hr.ApprovedByNavigation)
                .SingleOrDefaultAsync(hr => hr.UserId == userId, cancellationToken);
        }

        public async Task AddAsync(HostRegistration hostRegistration, CancellationToken cancellationToken = default)
        {
            await _db.HostRegistrations.AddAsync(hostRegistration, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(HostRegistration hostRegistration, CancellationToken cancellationToken = default)
        {
            _db.HostRegistrations.Update(hostRegistration);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var hostRegistration = await GetByIdAsync(id, cancellationToken);
            if (hostRegistration != null)
            {
                _db.HostRegistrations.Remove(hostRegistration);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<HostRegistration>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.HostRegistrations.ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<HostRegistration>> GetAllAsync(
            ApproveStatusFilter? approveFilter = null,
            HostRegistrationSort? sortBy = null,
            bool sortDesc = false,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _db.HostRegistrations
                .AsNoTracking()
                .Include(hr => hr.User)
                .AsQueryable();

            if (approveFilter.HasValue && approveFilter.Value != ApproveStatusFilter.Both)
            {
                if (approveFilter.Value == ApproveStatusFilter.Approved)
                {
                    query = query.Where(hr => hr.Approved == true);
                }
                else if (approveFilter.Value == ApproveStatusFilter.NotApproved)
                {
                    query = query.Where(hr => hr.Approved == false);
                }
            }

            sortBy ??= HostRegistrationSort.Id;

            switch (sortBy.Value)
            {
                case HostRegistrationSort.UserName:
                    query = sortDesc
                        ? query.OrderByDescending(hr => hr.User != null ? hr.User.Name : string.Empty)
                        : query.OrderBy(hr => hr.User != null ? hr.User.Name : string.Empty);
                    break;

                case HostRegistrationSort.Approved:
                    query = sortDesc
                        ? query.OrderByDescending(hr => hr.Approved)
                        : query.OrderBy(hr => hr.Approved);
                    break;

                case HostRegistrationSort.Id:
                default:
                    query = sortDesc ? query.OrderByDescending(hr => hr.Id) : query.OrderBy(hr => hr.Id);
                    break;
            }

            return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
