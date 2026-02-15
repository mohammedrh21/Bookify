using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Domain.Contracts.RefreshToken
{
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Adds a new refresh token
        /// </summary>
        Task AddAsync(Domain.Entities.RefreshToken token);

        /// <summary>
        /// Updates an existing refresh token
        /// </summary>
        Task UpdateAsync(Domain.Entities.RefreshToken token);

        /// <summary>
        /// Gets a refresh token by its hash
        /// </summary>
        Task<Domain.Entities.RefreshToken?> GetByTokenHashAsync(string tokenHash);

        /// <summary>
        /// Gets all active (non-revoked, non-expired) tokens for a user
        /// </summary>
        Task<IEnumerable<Domain.Entities.RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

        /// <summary>
        /// Revokes all tokens for a specific user
        /// </summary>
        Task RevokeAllUserTokensAsync(string userId);

        /// <summary>
        /// Deletes expired tokens (cleanup operation)
        /// </summary>
        Task DeleteExpiredTokensAsync();

        /// <summary>
        /// Saves changes to the database
        /// </summary>
        Task SaveChangesAsync();
    }
}
