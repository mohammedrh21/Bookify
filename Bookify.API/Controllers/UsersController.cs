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
    }
}
