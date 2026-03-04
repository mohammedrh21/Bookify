using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Contracts.SupportTicket
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Domain.Entities.SupportTicket>> GetAllAsync();
        Task AddAsync(Domain.Entities.SupportTicket SupportTicket);
        Task<Domain.Entities.SupportTicket?> GetByIdAsync(Guid id);
        Task<bool> IsExistsTodayAsync(string email, DateTime date);
        Task SaveChangesAsync();
    }
}
