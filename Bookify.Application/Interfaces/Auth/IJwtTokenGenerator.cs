using Bookify.Application.Common;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Bookify.Application.Interfaces.Auth
{
    public interface IJwtTokenGenerator
    {
        Task<string> GenerateTokenAsync(JwtUser user, IEnumerable<string> roles);
        string GenerateRefreshToken();
        string HashToken(string token);
    }
}
