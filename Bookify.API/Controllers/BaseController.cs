using Bookify.Application.Common;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bookify.API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        protected string CurrentUserId => GetClaimValue(ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub);

        /// <summary>
        /// Gets the current user's role from claims
        /// </summary>
        protected string CurrentUserRole => GetClaimValue(ClaimTypes.Role);

        /// <summary>
        /// Gets the current user's email from claims
        /// </summary>
        protected string CurrentUserEmail => GetClaimValue(ClaimTypes.Email, JwtRegisteredClaimNames.Email);

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        protected bool IsAdmin => CurrentUserRole == Roles.Admin;

        /// <summary>
        /// Checks if the current user is staff
        /// </summary>
        protected bool IsStaff => CurrentUserRole == Roles.Staff;

        /// <summary>
        /// Checks if the current user is a client
        /// </summary>
        protected bool IsClient => CurrentUserRole == Roles.Client;

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// Gets the user's ID as a Guid (returns empty Guid if invalid)
        /// </summary>
        protected Guid CurrentUserGuid
        {
            get
            {
                if (Guid.TryParse(CurrentUserId, out var guid))
                    return guid;
                return Guid.Empty;
            }
        }

        public BaseController()
        {
        }

        /// <summary>
        /// Reads the first available claim value from a priority list of claim types.
        /// </summary>
        protected string GetClaimValue(params string[] claimTypes)
        {
            foreach (var type in claimTypes)
            {
                var value = User.FindFirstValue(type);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Maps a ServiceResponse to an appropriate ActionResult.
        /// </summary>
        protected ActionResult HandleResult<T>(ServiceResponse<T> result, bool unwrapData = false)
        {
            if (result == null) 
                return NotFound();

            if (result.Success)
                return unwrapData && result.Data != null ? Ok(result.Data) : Ok(result);

            return BadRequest(unwrapData ? new { error = result.Message } : result);
        }
    }
}
