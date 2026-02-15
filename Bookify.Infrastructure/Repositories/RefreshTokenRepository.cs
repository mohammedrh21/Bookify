using Bookify.Domain.Contracts.RefreshToken;
using Bookify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Domain.Entities.RefreshToken token)
        {
            await _db.RefreshTokens.AddAsync(token);
        }

        public Task UpdateAsync(Domain.Entities.RefreshToken token)
        {
            _db.RefreshTokens.Update(token);
            return Task.CompletedTask;
        }

        public async Task<Domain.Entities.RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task<IEnumerable<Domain.Entities.RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await _db.RefreshTokens
                .Where(t => t.UserId == userId
                    && !t.IsRevoked
                    && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            _db.RefreshTokens.UpdateRange(tokens);
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var expiredTokens = await _db.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow.AddDays(-30)) // Keep for 30 days after expiry for audit
                .ToListAsync();

            _db.RefreshTokens.RemoveRange(expiredTokens);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
