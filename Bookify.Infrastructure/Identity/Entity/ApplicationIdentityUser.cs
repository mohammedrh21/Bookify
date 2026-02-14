using Microsoft.AspNetCore.Identity;

namespace Bookify.Infrastructure.Identity.Entity
{
    /// <summary>
    /// Represents Identity user
    /// </summary>
    public class ApplicationIdentityUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
