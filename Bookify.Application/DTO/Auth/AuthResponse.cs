using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = default!;
        public Guid UserId { get; set; }
    }
}
