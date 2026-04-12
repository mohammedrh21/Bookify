using Bookify.Domain.Entities;
using Bookify.Domain.Enums;

namespace Bookify.Domain.Contracts.Payment
{
    public interface IPaymentRepository : IGenericRepository<Entities.Payment>
    {
        Task<IEnumerable<Entities.Payment>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Entities.Payment>> GetPaymentsByServiceIdAsync(Guid serviceId);
    }
}
