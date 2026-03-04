using Bookify.Application.Common;
using Bookify.Application.DTO.Auth;
using Bookify.Application.DTO.Identity;
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

        /// <summary>Login with email and password.</summary>
        /// <response code="200">Login successful — returns tokens and user info.</response>
        /// <response code="401">Invalid credentials.</response>
        /// <response code="423">Account is locked.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        /// <summary>Refresh the access token using a refresh token.</summary>
        /// <response code="200">New tokens issued.</response>
        /// <response code="401">Refresh token invalid or expired.</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(result);
        }

        /// <summary>Revoke all refresh tokens for the current user (logout from all devices).</summary>
        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RevokeTokens()
        {
            var result = await _authService.RevokeTokenAsync(CurrentUserId);
            return Ok(result);
        }

        /// <summary>Register a new client account.</summary>
        /// <response code="200">Client registered — returns client ID.</response>
        /// <response code="400">Identity validation errors.</response>
        /// <response code="409">Email already registered.</response>
        [HttpPost("register/client")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterClient(RegisterClientRequest request)
        {
            var result = await _authService.RegisterClientAsync(request);
            return Ok(result);
        }

        /// <summary>Register a new staff account.</summary>
        /// <response code="200">Staff registered — returns staff ID.</response>
        /// <response code="400">Identity validation errors.</response>
        /// <response code="409">Email already registered.</response>
        [HttpPost("register/staff")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterStaff(RegisterStaffRequest request)
        {
            var result = await _authService.RegisterStaffAsync(request);
            return Ok(result);
        }

        /// <summary>Get current authenticated user information.</summary>
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
