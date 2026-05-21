using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment participant, CancellationToken cancellationToken = default);
        Task DeleteAsync(int participantId, int ticketId, CancellationToken cancellationToken = default);
        Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResultDto<Payment>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Payment?> GetByIdAsync(int participantId, int ticketId, CancellationToken cancellationToken = default);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(Payment participant, CancellationToken cancellationToken = default);
    }
}