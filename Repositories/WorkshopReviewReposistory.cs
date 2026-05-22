using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public class WorkshopReviewReposistory : IWorkshopReviewReposistory
    {
        private readonly ExeContext _db;

        public WorkshopReviewReposistory(ExeContext db)
        {
            _db = db;
        }

        public async Task<WorkshopReview?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopReviews
                .Include(wr => wr.User)
                .SingleOrDefaultAsync(wr => wr.Id == id, cancellationToken);
        }

        public async Task AddAsync(WorkshopReview workshopReview, CancellationToken cancellationToken = default)
        {
            await _db.WorkshopReviews.AddAsync(workshopReview, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(WorkshopReview workshopReview, CancellationToken cancellationToken = default)
        {
            _db.WorkshopReviews.Update(workshopReview);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var workshopReview = await GetByIdAsync(id, cancellationToken);
            if (workshopReview != null)
            {
                _db.WorkshopReviews.Remove(workshopReview);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<WorkshopReview>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopReviews.ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<WorkshopReview>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopReviews.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<PagedResultDto<WorkshopReview>> GetByWorkshopIdAsync(int workshopId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.WorkshopReviews
                .Where(wr => wr.WorkshopId == workshopId)
                .Include(wr => wr.User)
                .ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
