using Bookify.Application.DTO.Common;
using Bookify.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bookify.API.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    [Authorize]
    public class StatisticsController : BaseController
    {
        private readonly IBookingService _bookingService;

        public StatisticsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Admin dashboard — returns platform-wide statistics.
        /// Accessible only by Admin users.
        /// </summary>
        [HttpGet("admin/dashboard")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAdminDashboardStats(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _bookingService.GetAdminDashboardAsync(from, to);
            return HandleResult(result);
        }

        /// <summary>
        /// Staff dashboard — returns statistics scoped to the authenticated staff member.
        /// Accessible only by Staff users.
        /// </summary>
        [HttpGet("staff/dashboard")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> GetStaffDashboardStats(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var staffId = CurrentUserGuid;
            if (staffId == Guid.Empty)
                return Unauthorized(new { error = "Invalid staff identity." });

            var result = await _bookingService.GetStaffDashboardAsync(staffId, from, to);
            return HandleResult(result);
        }
    }
}
