using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Users;
using Bookify.Application.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : BaseController
    {
        private readonly IUserService _userService;

        public StaffController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a paginated list of clients who have booked the current staff member's service.
        /// </summary>
        [HttpGet("clients")]
        [ProducesResponseType(typeof(ServiceResponse<PagedResult<StaffClientDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStaffClients(
            [FromQuery] string? search = null,
            [FromQuery] DateTime? dateFilter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var staffId = CurrentUserGuid;
            if (staffId == Guid.Empty)
                return Unauthorized(new { error = "Invalid staff identity." });

            var result = await _userService.GetStaffClientsAsync(staffId, search, dateFilter, page, pageSize);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves detailed statistics for a specific client focused only on the current staff member's service.
        /// </summary>
        [HttpGet("clients/{clientId}/details")]
        [ProducesResponseType(typeof(ServiceResponse<StaffClientDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStaffClientDetails(Guid clientId)
        {
            var staffId = CurrentUserGuid;
            if (staffId == Guid.Empty)
                return Unauthorized(new { error = "Invalid staff identity." });

            var result = await _userService.GetStaffClientDetailsAsync(staffId, clientId);
            return HandleResult(result);
        }
    }
}
