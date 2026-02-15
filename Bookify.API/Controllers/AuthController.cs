using Bookify.Application.Common;
using Bookify.Application.DTO;
using Bookify.Application.DTO.Auth;
using Bookify.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Access token, refresh token, and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Revoke all refresh tokens for the current user (logout from all devices)
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RevokeTokens()
        {
            var result = await _authService.RevokeTokenAsync(CurrentUserId);
            return Ok(result);
        }

        /// <summary>
        /// Register a new client account
        /// </summary>
        /// <param name="request">Client registration information</param>
        /// <returns>Created client ID</returns>
        [HttpPost("register/client")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterClient(RegisterClientRequest request)
        {
            var result = await _authService.RegisterClientAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Register a new staff account
        /// </summary>
        /// <param name="request">Staff registration information</param>
        /// <returns>Created staff ID</returns>
        [HttpPost("register/staff")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterStaff(RegisterStaffRequest request)
        {
            var result = await _authService.RegisterStaffAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                userId = CurrentUserId,
                email = CurrentUserEmail,
                role = CurrentUserRole,
                isAuthenticated = IsAuthenticated
            });
        }
    }
}
