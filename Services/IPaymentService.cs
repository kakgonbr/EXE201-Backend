
using EXE201_Backend.Models.Dto;

namespace EXE201_Backend.Services
{
    public interface IPaymentService
    {
        Task<string?> GetPaymentStatusAsync(int userId, int ticketId, CancellationToken cancellationToken = default);
        Task<bool> InformPaymentStatus(int userId, int ticketId, decimal amountPaid, CancellationToken cancellationToken = default);
        Task<PaymentInfoDto?> StartCheckout(int userId, int ticketId, CancellationToken cancellationToken = default);
    }
}