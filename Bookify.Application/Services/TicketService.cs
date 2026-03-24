using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.SupportTicket;
using Bookify.Application.DTO.Common;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Ticket;
using Bookify.Domain.Contracts.SupportTicket;
using Bookify.Domain.Entities;
using Bookify.Domain.Exceptions;

namespace Bookify.Application.Services
{
    public sealed class TicketService : ITicketService
    {
        private readonly ITicketRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<TicketService> _logger;

        public TicketService(
            ITicketRepository repo,
            IMapper mapper,
            IAppLogger<TicketService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<PagedResult<TicketResponse>>> GetAllAsync(PaginationParams paginationParams)
        {
            _logger.LogInformation($"Fetching support tickets: Page {paginationParams.PageNumber}, Size {paginationParams.PageSize}");
            var (tickets, totalCount) = await _repo.GetPaginatedAsync(paginationParams.PageNumber, paginationParams.PageSize);
            
            var pagedResult = new PagedResult<TicketResponse>
            {
                Items = _mapper.Map<IEnumerable<TicketResponse>>(tickets),
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };

            return ServiceResponse<PagedResult<TicketResponse>>.Ok(data: pagedResult);
        }

        public async Task<ServiceResponse<Guid>> SubmitAsync(CreateTicketRequest request)
        {
            _logger.LogInformation($"Submitting ticket for {request.Email}");

            var today = DateTime.UtcNow.Date;
            
            if (await _repo.IsExistsTodayAsync(request.Email, today))
            {
                 throw new TicketRateLimitException();
            }

            var ticket = new SupportTicket
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Subject = request.Subject.Trim(),
                Description = request.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(ticket);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Ticket submitted: {ticket.Id}");

            return ServiceResponse<Guid>.Ok(
                id: ticket.Id,
                data: ticket.Id,
                message: "Support ticket submitted successfully.");
        }
    
        public async Task<ServiceResponse<bool>> MarkAsReadAsync(Guid id)
        {
            _logger.LogInformation($"Marking ticket {id} as read");
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket == null)
            {
                return ServiceResponse<bool>.Fail("Ticket not found.");
            }

            if (!ticket.IsRead)
            {
                ticket.IsRead = true;
                await _repo.SaveChangesAsync();
            }

            return ServiceResponse<bool>.Ok(data: true);
        }
    }
}
