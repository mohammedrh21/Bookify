using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime Expiration { get; set; }
        public string Role { get; set; } = default!;
        public Guid UserId { get; set; }
    }
}
