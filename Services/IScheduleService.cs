using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Services
{
    public interface IScheduleService
    {
        Task<WorkshopScheduleDetailsDto> GetScheduleDetailsAsync(int scheduleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkshopScheduleDto>> GetSchedulesInMonthAsync(int userId, int month, CancellationToken cancellationToken = default);
        Task<bool> IsUserOccupiedAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
    }
}