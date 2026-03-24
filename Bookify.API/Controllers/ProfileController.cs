using Bookify.Application.DTO.Identity;
using Bookify.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [ApiController]

    [Route("api/profile")]
    public class ProfileController : BaseController
    {
        private readonly IIdentityUserService _identityService;

        public ProfileController(IIdentityUserService identityService)
        {
            _identityService = identityService;
        }

        // ── Client ────────────────────────────────────────────────────────────

        [HttpGet("client")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> GetClientProfile()
        {
            var result = await _identityService.GetClientProfileAsync(CurrentUserId);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPut("client")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> UpdateClientProfile([FromBody] UpdateClientProfileRequest request)
        {
            var result = await _identityService.UpdateClientProfileAsync(CurrentUserId, request);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPost("client/change-password")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> ChangeClientPassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _identityService.ChangePasswordAsync(CurrentUserId, request);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPost("client/upload-image")]
        [Authorize(Policy = "ClientOnly")]
        public async Task<IActionResult> UploadClientImage(IFormFile file)
        {
            var result = await _identityService.UpdateProfileImageAsync(CurrentUserId, file, "Client");
            if (!result.Success) return BadRequest(new { error = result.Message });
            return Ok(new { imageUrl = result.Data, message = result.Message });
        }

        // ── Staff ─────────────────────────────────────────────────────────────

        [HttpGet("staff")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> GetStaffProfile()
        {
            var result = await _identityService.GetStaffProfileAsync(CurrentUserId);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPut("staff")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> UpdateStaffProfile([FromBody] UpdateStaffProfileRequest request)
        {
            var result = await _identityService.UpdateStaffProfileAsync(CurrentUserId, request);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPost("staff/change-password")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> ChangeStaffPassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _identityService.ChangePasswordAsync(CurrentUserId, request);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPost("staff/upload-image")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> UploadStaffImage(IFormFile file)
        {
            var result = await _identityService.UpdateProfileImageAsync(CurrentUserId, file, "Staff");
            if (!result.Success) return BadRequest(new { error = result.Message });
            return Ok(new { imageUrl = result.Data, message = result.Message });
        }

        // ── Admin ─────────────────────────────────────────────────────────────
        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAdminProfile()
        {
            var result = await _identityService.GetAdminProfileAsync(CurrentUserId);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPut("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateAdminProfile([FromBody] UpdateAdminProfileRequest request)
        {
            var result = await _identityService.UpdateAdminProfileAsync(CurrentUserId, request);
            return HandleResult(result, unwrapData: true);
        }

        [HttpPost("admin/change-password")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ChangeAdminPassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _identityService.ChangePasswordAsync(CurrentUserId, request);
            return HandleResult(result, unwrapData: true);
        }
    }
}
