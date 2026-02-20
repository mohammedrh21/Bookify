using Bookify.Application.Common;
using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookify.API.Controllers
{
    /// <summary>
    /// Manages bookings lifecycle: create, confirm, complete, cancel.
    /// </summary>
    [Route("api/bookings")]
    [Authorize]
    [Produces("application/json")]
    public class BookingsController : BaseController
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        /// <summary>Create a new booking (Client only).</summary>
        /// <response code="201">Booking created.</response>
        /// <response code="409">Time slot unavailable.</response>
        /// <response code="422">Booking date must be in the future.</response>
        [HttpPost]
        [Authorize(Roles = Roles.Client)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            // Clients can only create bookings for themselves
            if (!IsAdmin && request.ClientId.ToString() != CurrentUserId)
                return Forbid();

            var result = await _bookingService.CreateAsync(request);
            return CreatedAtAction(nameof(GetClientBookings), new { clientId = request.ClientId }, result);
        }

        /// <summary>Cancel a booking (Client or Staff).</summary>
        /// <response code="200">Booking cancelled.</response>
        /// <response code="403">Not the booking owner or assigned staff.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="422">Booking cannot be cancelled from current status.</response>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request)
        {
            request.BookingId = id;
            var result = await _bookingService.CancelAsync(request);
            return Ok(result);
        }

        /// <summary>Confirm a booking (Staff only).</summary>
        /// <response code="200">Booking confirmed.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="422">Booking is not in Pending status.</response>
        [HttpPost("{id:guid}/confirm")]
        [Authorize(Roles = Roles.Staff)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await _bookingService.ConfirmAsync(id);
            return Ok(result);
        }

        /// <summary>Complete a booking (Staff only).</summary>
        /// <response code="200">Booking completed.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="422">Booking is not in Confirmed status.</response>
        [HttpPost("{id:guid}/complete")]
        [Authorize(Roles = Roles.Staff)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await _bookingService.CompleteAsync(id);
            return Ok(result);
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        /// <summary>Get bookings for a specific client.</summary>
        /// <response code="200">Client's bookings.</response>
        /// <response code="403">Cannot view another client's bookings.</response>
        [HttpGet("client/{clientId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetClientBookings(Guid clientId)
        {
            if (!IsAdmin && CurrentUserId != clientId.ToString())
                return Forbid();

            var result = await _bookingService.GetClientBookingsAsync(clientId);
            return Ok(result);
        }

        /// <summary>Get bookings for a specific staff member.</summary>
        /// <response code="200">Staff's bookings.</response>
        /// <response code="403">Cannot view another staff member's bookings.</response>
        [HttpGet("staff/{staffId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStaffBookings(Guid staffId)
        {
            if (!IsAdmin && CurrentUserId != staffId.ToString())
                return Forbid();

            var result = await _bookingService.GetStaffBookingsAsync(staffId);
            return Ok(result);
        }

        /// <summary>Get all bookings with optional filters (Admin only).</summary>
        /// <response code="200">All bookings (paginated).</response>
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? status = null)
        {
            Domain.Enums.BookingStatus? parsed = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<Domain.Enums.BookingStatus>(status, ignoreCase: true, out var s))
            {
                parsed = s;
            }

            var result = await _bookingService.GetAllAsync(from, to, parsed);
            return Ok(result);
        }
    }
}
