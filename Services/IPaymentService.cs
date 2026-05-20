
using EXE201_Backend.Models.Responses;

namespace EXE201_Backend.Services
{
    public interface IPaymentService
    {
        Task<bool> InformPaymentStatus(int userId, int ticketId, decimal amountPaid, CancellationToken cancellationToken = default);
        Task<PaymentInfo?> StartCheckout(int userId, int ticketId, CancellationToken cancellationToken = default);
    }
}