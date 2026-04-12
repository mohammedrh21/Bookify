using Bookify.Application.Common;
using Bookify.Application.DTO.Common;
using Bookify.Application.DTO.Users;
using Bookify.Application.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            return Ok(await _userService.GetStaffPaginatedAsync(page, pageSize));
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            return Ok(await _userService.GetClientsPaginatedAsync(page, pageSize));
        }

        [HttpPost("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var result = await _userService.ToggleUserActiveAsync(id);
            if (!result.Success) return HandleResult(result);
            return Ok(new { IsActive = result.Data });
        }

        [HttpGet("clients/{id}/report")]
        public async Task<IActionResult> GetClientReport(Guid id)
        {
            var result = await _userService.GetClientReportAsync(id);
            return HandleResult(result);
        }

        [HttpGet("admin-clients")]
        [ProducesResponseType(typeof(ServiceResponse<PagedResult<AdminClientDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminClients(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAdminClientsAsync(search, page, pageSize);
            return HandleResult(result);
        }

        [HttpGet("clients/{id}/admin-details")]
        [ProducesResponseType(typeof(ServiceResponse<AdminClientDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAdminClientDetails(Guid id)
        {
            var result = await _userService.GetAdminClientDetailsAsync(id);
            return HandleResult(result);
        }

        // ── Admin Staff Members ────────────────────────────────────────────────

        [HttpGet("admin-staff")]
        [ProducesResponseType(typeof(ServiceResponse<PagedResult<AdminStaffDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminStaff(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAdminStaffAsync(search, page, pageSize);
            return HandleResult(result);
        }

        [HttpGet("staff/{id}/admin-details")]
        [ProducesResponseType(typeof(ServiceResponse<AdminStaffDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAdminStaffDetails(Guid id)
        {
            var result = await _userService.GetAdminStaffDetailsAsync(id);
            return HandleResult(result);
        }
    }
}
