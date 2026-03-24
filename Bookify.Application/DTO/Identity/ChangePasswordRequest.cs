using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.DTO.Identity
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = default!;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = default!;

        [Required, Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = default!;
    }
}
