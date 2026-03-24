using Microsoft.AspNetCore.Identity;

namespace Bookify.Infrastructure.Identity.Entity
{
    /// <summary>
    /// Represents Identity user
    /// </summary>
    public class ApplicationIdentityUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Forgot Password OTP
        public string? ResetOtp { get; set; }
        public DateTime? ResetOtpExpiry { get; set; }
    }
}
