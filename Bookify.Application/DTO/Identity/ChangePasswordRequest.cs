namespace Bookify.Application.DTO.Identity
{
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = default!;

        public string NewPassword { get; set; } = default!;

        public string ConfirmPassword { get; set; } = default!;
    }
}
