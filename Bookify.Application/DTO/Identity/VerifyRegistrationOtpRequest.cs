namespace Bookify.Application.DTO.Identity
{
    public class VerifyRegistrationOtpRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Otp { get; set; } = string.Empty;
    }
}
