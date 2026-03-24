using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.DTO.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
