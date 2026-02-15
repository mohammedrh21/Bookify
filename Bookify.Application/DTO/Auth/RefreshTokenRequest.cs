using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = default!;
    }
}
