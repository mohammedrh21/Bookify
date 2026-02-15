using Bookify.Application.Common;
using Bookify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Bookify.Application.Interfaces.Auth
{

    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Generates a JWT access token for the specified user
        /// </summary>
        Task<string> GenerateTokenAsync(JwtUser user, IEnumerable<string> roles);

        /// <summary>
        /// Generates a new refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Hashes a token for secure storage
        /// </summary>
        string HashToken(string token);

        /// <summary>
        /// Saves a refresh token to the database
        /// </summary>
        Task SaveRefreshTokenAsync(RefreshToken token);

        /// <summary>
        /// Retrieves a refresh token by its hash
        /// </summary>
        Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);

        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        Task RevokeTokenAsync(string tokenHash);

        /// <summary>
        /// Validates a refresh token (checks expiration and revocation)
        /// </summary>
        Task<RefreshToken?> ValidateRefreshTokenAsync(string tokenHash);

        /// <summary>
        /// Revokes all refresh tokens for a specific user
        /// </summary>
        Task RevokeAllUserTokensAsync(string userId);
    }
}

