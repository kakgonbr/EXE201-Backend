using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Repositories
{
    public interface IWorkshopScheduleRepository
    {
        Task AddAsync(WorkshopSchedule schedule, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkshopSchedule>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopSchedule>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<WorkshopSchedule?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkshopSchedule>> GetSchedulesInMonthAsync(int userId, int month, CancellationToken cancellationToken = default);
        Task<PagedResultDto<WorkshopSchedule>> GetUpcoming(int userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<bool> IsUserOccupiedAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(WorkshopSchedule schedule, CancellationToken cancellationToken = default);
    }
}