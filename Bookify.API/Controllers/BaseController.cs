using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Bookify.API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        protected string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? "";

        public BaseController()
        {

        }

    }
}
