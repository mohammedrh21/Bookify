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
            this._authService = authService;
        }

        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        ///     Register a new client
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register/client")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterClient(RegisterClientRequest request)
        {
            var result = await _authService.RegisterClientAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        ///     Register a new Staff
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register/staff")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterStaff(RegisterStaffRequest request)
        {
            var result = await _authService.RegisterStaffAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
