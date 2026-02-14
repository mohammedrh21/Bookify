using Bookify.Application.DTO.Booking;
using Bookify.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookify.API.Controllers
{
    [Route("api/bookings")]
    public class BookingsController : BaseController
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(CreateBookingRequest request)
        {
            var result = await _bookingService.CreateAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelBookingRequest request)
        {
            request.BookingId = id;

            var result = await _bookingService.CancelAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var result = await _bookingService.ConfirmAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Complete(Guid id)
        {
            var result = await _bookingService.CompleteAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> ClientBookings(Guid clientId)
            => Ok(await _bookingService.GetClientBookingsAsync(clientId));

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> StaffBookings(Guid staffId)
            => Ok(await _bookingService.GetStaffBookingsAsync(staffId));
    }
}
