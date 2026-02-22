namespace Bookify.Application.DTO.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime Expiration { get; set; }
        public string Role { get; set; } = default!;
        public Guid UserId { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
