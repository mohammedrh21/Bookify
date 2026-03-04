using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.SupportTicket;
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

        public async Task<ServiceResponse<IEnumerable<TicketResponse>>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all support tickets");
            var tickets = await _repo.GetAllAsync();
            var sortedTickets = tickets.OrderByDescending(t => t.CreatedAt).ToList();

            return ServiceResponse<IEnumerable<TicketResponse>>.Ok(
                data: _mapper.Map<IEnumerable<TicketResponse>>(sortedTickets));
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
    }
}
