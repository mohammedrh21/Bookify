using Bookify.Application.DTO.SupportTicket;
using Bookify.Application.Interfaces.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [Route("api/tickets")]
    [Produces("application/json")]
    public class SupportTicketController : BaseController
    {
        private readonly ITicketService _ticketService;

        public SupportTicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _ticketService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Submit([FromBody] CreateTicketRequest request)
        {
            var result = await _ticketService.SubmitAsync(request);
            return Ok(result); // OK since it's just submitting and we don't have a GET single ticket
        }
    }
}
