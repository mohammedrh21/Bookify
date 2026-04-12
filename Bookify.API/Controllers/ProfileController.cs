using Bookify.Application.Common;
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

        /// <summary>
        /// Retrieves the profile information for the current client.
        /// </summary>
        /// <returns>A response containing the client's profile data.</returns>
        /// <response code="200">The profile was successfully retrieved.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Client" role.</response>
        [HttpGet("client")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(ServiceResponse<ClientProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetClientProfile()
        {
            var result = await _identityService.GetClientProfileAsync(CurrentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates the profile information for the current client.
        /// </summary>
        /// <param name="request">The updated profile information.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <response code="200">The profile was successfully updated.</response>
        /// <response code="400">The update request was invalid or failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Client" role.</response>
        [HttpPut("client")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateClientProfile([FromBody] UpdateClientProfileRequest request)
        {
            var result = await _identityService.UpdateClientProfileAsync(CurrentUserId, request);
            return HandleResult(result);
        }

        /// <summary>
        /// Changes the password for the current client.
        /// </summary>
        /// <param name="request">The password change request.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <response code="200">The password was successfully changed.</response>
        /// <response code="400">The request was invalid or the change failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Client" role.</response>
        [HttpPost("client/change-password")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeClientPassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _identityService.ChangePasswordAsync(CurrentUserId, request);
            return HandleResult(result);
        }

        /// <summary>
        /// Uploads a new profile image for the current client.
        /// </summary>
        /// <param name="file">The image file to upload.</param>
        /// <returns>A response containing the URL of the uploaded image.</returns>
        /// <response code="200">The image was successfully uploaded.</response>
        /// <response code="400">The upload failed or the file was invalid.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Client" role.</response>
        [HttpPost("client/upload-image")]
        [Authorize(Policy = "ClientOnly")]
        [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadClientImage(IFormFile file)
        {
            var result = await _identityService.UpdateProfileImageAsync(CurrentUserId, file, "Client");
            return HandleResult(result);
        }

        // ── Staff ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves the profile information for the current staff member.
        /// </summary>
        /// <returns>A response containing the staff member's profile data.</returns>
        /// <response code="200">The profile was successfully retrieved.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Staff" role.</response>
        [HttpGet("staff")]
        [Authorize(Policy = "StaffOnly")]
        [ProducesResponseType(typeof(ServiceResponse<StaffProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStaffProfile()
        {
            var result = await _identityService.GetStaffProfileAsync(CurrentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates the profile information for the current staff member.
        /// </summary>
        /// <param name="request">The updated profile information.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <response code="200">The profile was successfully updated.</response>
        /// <response code="400">The update request was invalid or failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Staff" role.</response>
        [HttpPut("staff")]
        [Authorize(Policy = "StaffOnly")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStaffProfile([FromBody] UpdateStaffProfileRequest request)
        {
            var result = await _identityService.UpdateStaffProfileAsync(CurrentUserId, request);
            return HandleResult(result);
        }

        /// <summary>
        /// Changes the password for the current staff member.
        /// </summary>
        /// <param name="request">The password change request.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <response code="200">The password was successfully changed.</response>
        /// <response code="400">The request was invalid or the change failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Staff" role.</response>
        [HttpPost("staff/change-password")]
        [Authorize(Policy = "StaffOnly")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeStaffPassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _identityService.ChangePasswordAsync(CurrentUserId, request);
            return HandleResult(result);
        }

        /// <summary>
        /// Uploads a new profile image for the current staff member.
        /// </summary>
        /// <param name="file">The image file to upload.</param>
        /// <returns>A response containing the URL of the uploaded image.</returns>
        /// <response code="200">The image was successfully uploaded.</response>
        /// <response code="400">The upload failed or the file was invalid.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Staff" role.</response>
        [HttpPost("staff/upload-image")]
        [Authorize(Policy = "StaffOnly")]
        [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadStaffImage(IFormFile file)
        {
            var result = await _identityService.UpdateProfileImageAsync(CurrentUserId, file, "Staff");
            return HandleResult(result);
        }

        // ── Admin ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves the profile information for the current administrator.
        /// </summary>
        /// <returns>A response containing the administrator's profile data.</returns>
        /// <response code="200">The profile was successfully retrieved.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Admin" role.</response>
        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ServiceResponse<AdminProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAdminProfile()
        {
            var result = await _identityService.GetAdminProfileAsync(CurrentUserId);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates the profile information for the current administrator.
        /// </summary>
        /// <param name="request">The updated profile information.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <response code="200">The profile was successfully updated.</response>
        /// <response code="400">The update request was invalid or failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Admin" role.</response>
        [HttpPut("admin")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateAdminProfile([FromBody] UpdateAdminProfileRequest request)
        {
            var result = await _identityService.UpdateAdminProfileAsync(CurrentUserId, request);
            return HandleResult(result);
        }

        /// <summary>
        /// Changes the password for the current administrator.
        /// </summary>
        /// <param name="request">The password change request.</param>
        /// <returns>A response indicating success or failure.</returns>
        /// <response code="200">The password was successfully changed.</response>
        /// <response code="400">The request was invalid or the change failed.</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="403">The user does not have the "Admin" role.</response>
        [HttpPost("admin/change-password")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeAdminPassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _identityService.ChangePasswordAsync(CurrentUserId, request);
            return HandleResult(result);
        }
    }
}
