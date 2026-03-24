using Bookify.Application.Common;
using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Bookify.Domain.Exceptions;
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
            var result = await _bookingService.CreateAsync(request);
            return HandleResult(result);
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
            return HandleResult(result);
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
            return HandleResult(result);
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
            return HandleResult(result);
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
            var result = await _bookingService.GetClientBookingsAsync(clientId);
            return HandleResult(result);
        }

        [HttpGet("staff/{staffId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStaffBookings(
            Guid staffId,
            [FromQuery] string?   status  = null,
            [FromQuery] DateTime? from    = null,
            [FromQuery] DateTime? to      = null,
            [FromQuery] string?   search  = null,
            [FromQuery] bool      sortAsc = true,
            [FromQuery] int       page    = 1,
            [FromQuery] int       pageSize = 10)
        {
            Domain.Enums.BookingStatus? parsed = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<Domain.Enums.BookingStatus>(status, ignoreCase: true, out var s))
            {
                parsed = s;
            }

            var result = await _bookingService.GetStaffBookingsPagedAsync(
                staffId, parsed, from, to, search, sortAsc, page, pageSize);
            return HandleResult(result);
        }

        /// <summary>Get occupied slots for a service in a date range.</summary>
        [HttpGet("service/{serviceId:guid}/occupied-slots")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOccupiedSlots(
            Guid serviceId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _bookingService.GetOccupiedSlotsAsync(serviceId, from, to);
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _bookingService.GetByIdAsync(id);
            return HandleResult(result);
        }

        /// <summary>Get all bookings with optional filters (Admin only).</summary>
        /// <response code="200">All bookings (paginated).</response>
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from       = null,
            [FromQuery] DateTime? to         = null,
            [FromQuery] string?   status     = null,
            [FromQuery] string?   search     = null,
            [FromQuery] string?   staffName  = null,
            [FromQuery] Guid?     categoryId = null,
            [FromQuery] int       page       = 1,
            [FromQuery] int       pageSize   = 10)
        {
            Domain.Enums.BookingStatus? parsed = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<Domain.Enums.BookingStatus>(status, ignoreCase: true, out var s))
            {
                parsed = s;
            }

            var result = await _bookingService.GetAllAsync(from, to, parsed, search, staffName, categoryId, page, pageSize);
            return HandleResult(result);
        }
    }
}
