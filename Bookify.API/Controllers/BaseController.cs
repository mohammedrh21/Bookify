using Bookify.Application.Common;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;
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
        protected string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            string.Empty;

        /// <summary>
        /// Gets the current user's role from claims
        /// </summary>
        protected string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        /// <summary>
        /// Gets the current user's email from claims
        /// </summary>
        protected string CurrentUserEmail =>
            User.FindFirstValue(ClaimTypes.Email) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Email) ??
            string.Empty;

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
    }
}
