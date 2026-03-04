using Bookify.Application.Common;
using Bookify.Application.DTO.SupportTicket;

namespace Bookify.Application.Interfaces.Ticket
{
    public interface ITicketService
    {
        Task<ServiceResponse<IEnumerable<TicketResponse>>> GetAllAsync();
        Task<ServiceResponse<Guid>> SubmitAsync(CreateTicketRequest request);
    }
}
