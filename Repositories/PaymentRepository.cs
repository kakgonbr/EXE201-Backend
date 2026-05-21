using EXE201_Backend.Data;
using EXE201_Backend.Extensions;
using EXE201_Backend.Models;
using EXE201_Backend.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ExeContext _db;

        public PaymentRepository(ExeContext db)
        {
            _db = db;
        }

        public async Task<Payment?> GetByIdAsync(int participantId, int ticketId, CancellationToken cancellationToken = default)
        {
            return await _db.Payments
                .SingleOrDefaultAsync(p => p.ParticipantId == participantId && p.TicketId == ticketId, cancellationToken);
        }

        public async Task AddAsync(Payment participant, CancellationToken cancellationToken = default)
        {
            await _db.Payments.AddAsync(participant, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> UpdateAsync(Payment participant, CancellationToken cancellationToken = default)
        {
            _db.Payments.Update(participant);
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int participantId, int ticketId, CancellationToken cancellationToken = default)
        {
            var participant = await GetByIdAsync(participantId, ticketId, cancellationToken);
            if (participant != null)
            {
                _db.Payments.Remove(participant);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Payments.ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<Payment>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _db.Payments.ToPagedResultAsync(page, pageSize, cancellationToken);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
