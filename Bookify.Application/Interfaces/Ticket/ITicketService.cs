using Bookify.Application.Common;
using Bookify.Application.DTO.SupportTicket;
using Bookify.Application.DTO.Common;

namespace Bookify.Application.Interfaces.Ticket
{
    public interface ITicketService
    {
        Task<ServiceResponse<PagedResult<TicketResponse>>> GetAllAsync(PaginationParams paginationParams);
        Task<ServiceResponse<Guid>> SubmitAsync(CreateTicketRequest request);
        Task<ServiceResponse<bool>> MarkAsReadAsync(Guid id);
    }
}
