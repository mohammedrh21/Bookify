using Bookify.Application.Common;
using Bookify.Application.Interfaces.Auth;
using Bookify.Domain.Contracts.RefreshToken;
using Bookify.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bookify.Infrastructure.Services.Auth
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepo;

        public JwtTokenGenerator(
            IConfiguration configuration,
            IRefreshTokenRepository refreshTokenRepo)
        {
            _configuration = configuration;
            _refreshTokenRepo = refreshTokenRepo;
        }

        public Task<string> GenerateTokenAsync(JwtUser user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles.Distinct())
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task SaveRefreshTokenAsync(RefreshToken token)
        {
            await _refreshTokenRepo.AddAsync(token);
            await _refreshTokenRepo.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
        {
            return await _refreshTokenRepo.GetByTokenHashAsync(tokenHash);
        }

        public async Task RevokeTokenAsync(string tokenHash)
        {
            var token = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash);
            if (token != null && !token.IsRevoked)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepo.UpdateAsync(token);
                await _refreshTokenRepo.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken?> ValidateRefreshTokenAsync(string tokenHash)
        {
            var token = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash);

            if (token == null)
                return null;

            if (token.IsRevoked)
                return null;

            if (token.ExpiresAt < DateTime.UtcNow)
                return null;

            return token;
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            await _refreshTokenRepo.RevokeAllUserTokensAsync(userId);
        }
    }
}
