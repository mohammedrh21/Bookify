using Bookify.Application.Common;
using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookify.API.Controllers
{
    [Route("api/bookings")]
    [Authorize]
    public class BookingsController : BaseController
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Create a new booking (Client only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = Roles.Client)]
        public async Task<IActionResult> Create(CreateBookingRequest request)
        {
            // Ensure client can only create bookings for themselves
            if (request.ClientId.ToString() != CurrentUserId && !IsAdmin)
            {
                return Forbid();
            }

            var result = await _bookingService.CreateAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request)
        {
            request.BookingId = id;

            var result = await _bookingService.CancelAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Confirm a booking (Staff only)
        /// </summary>
        [HttpPost("{id}/confirm")]
        [Authorize(Roles = Roles.Staff)]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await _bookingService.ConfirmAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Complete a booking (Staff only)
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = Roles.Staff)]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await _bookingService.CompleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get bookings for a specific client
        /// </summary>
        [HttpGet("client/{clientId}")]
        [Authorize]
        public async Task<IActionResult> ClientBookings(Guid clientId)
        {
            // Clients can only view their own bookings, admins can view any
            if (CurrentUserRole != Roles.Admin && CurrentUserId != clientId.ToString())
            {
                return Forbid();
            }

            var result = await _bookingService.GetClientBookingsAsync(clientId);
            return Ok(result);
        }

        /// <summary>
        /// Get bookings for a specific staff member
        /// </summary>
        [HttpGet("staff/{staffId}")]
        [Authorize]
        public async Task<IActionResult> StaffBookings(Guid staffId)
        {
            // Staff can only view their own bookings, admins can view any
            if (CurrentUserRole != Roles.Admin && CurrentUserId != staffId.ToString())
            {
                return Forbid();
            }

            var result = await _bookingService.GetStaffBookingsAsync(staffId);
            return Ok(result);
        }

        /// <summary>
        /// Get all bookings (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? status = null)
        {
            Domain.Enums.BookingStatus? bookingStatus = null;
            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<Domain.Enums.BookingStatus>(status, true, out var parsed))
            {
                bookingStatus = parsed;
            }

            var result = await _bookingService.GetAllAsync(from, to, bookingStatus);
            return Ok(result);
        }
    }
}
