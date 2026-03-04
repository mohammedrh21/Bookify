using Bookify.Application.Common;
using Bookify.Application.DTO.Auth;
using Bookify.Application.DTO.Identity;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Auth;
using Bookify.Application.Interfaces.Client;
using Bookify.Application.Interfaces.Staff;
using Bookify.Domain.Entities;
using Bookify.Domain.Exceptions;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;

namespace Bookify.Infrastructure.Services.Auth
{
    /// <summary>
    /// Infrastructure implementation of <see cref="IAuthService"/>.
    /// All error paths throw typed <see cref="DomainException"/>-derived exceptions
    /// that are caught uniformly by <c>GlobalExceptionMiddleware</c>.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly IClientRepository _clientRepo;
        private readonly IStaffRepository _staffRepo;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            UserManager<ApplicationIdentityUser> userManager,
            IClientRepository clientRepo,
            IStaffRepository staffRepo,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _clientRepo = clientRepo;
            _staffRepo = staffRepo;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        // ─────────────────────────────────────────────
        // Registration
        // ─────────────────────────────────────────────

        /// <exception cref="ConflictException">When the email is already registered.</exception>
        /// <exception cref="RegistrationFailedException">When ASP.NET Identity validation fails.</exception>
        public async Task<ServiceResponse<Guid>> RegisterClientAsync(RegisterClientRequest request)
        {
            var identityUserId = await CreateIdentityUserAsync(request, Roles.Client);

            var client = new Client
            {
                Id = identityUserId,
                FullName = request.FullName,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth
            };

            await _clientRepo.AddAsync(client);

            return ServiceResponse<Guid>.Ok(
                id: client.Id,
                message: "Client registered successfully.");
        }

        /// <exception cref="ConflictException">When the email is already registered.</exception>
        /// <exception cref="RegistrationFailedException">When ASP.NET Identity validation fails.</exception>
        public async Task<ServiceResponse<Guid>> RegisterStaffAsync(RegisterStaffRequest request)
        {
            var identityUserId = await CreateIdentityUserAsync(request, Roles.Staff);

            var staff = new Staff
            {
                Id = identityUserId,
                FullName = request.FullName,
                Phone = request.Phone
            };

            await _staffRepo.AddAsync(staff);

            return ServiceResponse<Guid>.Ok(
                id: staff.Id,
                message: "Staff registered successfully.");
        }

        // ─────────────────────────────────────────────
        // Login / Token management
        // ─────────────────────────────────────────────

        /// <exception cref="InvalidCredentialsException">When email or password is wrong.</exception>
        /// <exception cref="UserLockedException">When the account is locked out.</exception>
        public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var identityUser = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new InvalidCredentialsException();

            if (!await _userManager.CheckPasswordAsync(identityUser, request.Password))
                throw new InvalidCredentialsException();

            if (await _userManager.IsLockedOutAsync(identityUser))
                throw new UserLockedException();

            var jwtUser = new JwtUser
            {
                Id = identityUser.Id,
                Email = identityUser.Email!,
                UserName = identityUser.UserName!
            };

            var roles = await _userManager.GetRolesAsync(identityUser);
            var accessToken = await _jwtTokenGenerator.GenerateTokenAsync(jwtUser, roles);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenHash = _jwtTokenGenerator.HashToken(refreshToken);

            await _jwtTokenGenerator.SaveRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = identityUser.Id.ToString(),
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            });

            await _userManager.ResetAccessFailedCountAsync(identityUser);

            return ServiceResponse<LoginResponse>.Ok(
                new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Role = roles.FirstOrDefault() ?? "User",
                    UserId = identityUser.Id,
                    FullName = identityUser.FullName ?? identityUser.UserName ?? string.Empty,
                    Email = identityUser.Email ?? string.Empty
                },
                "Login successful.");
        }

        /// <exception cref="InvalidCredentialsException">When the refresh token is invalid or expired.</exception>
        public async Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var tokenHash = _jwtTokenGenerator.HashToken(request.RefreshToken);
            var storedToken = await _jwtTokenGenerator.ValidateRefreshTokenAsync(tokenHash)
                ?? throw new InvalidCredentialsException("Invalid or expired refresh token.");

            var identityUser = await _userManager.FindByIdAsync(storedToken.UserId)
                ?? throw new InvalidCredentialsException("User associated with token not found.");

            await _jwtTokenGenerator.RevokeTokenAsync(tokenHash);

            var jwtUser = new JwtUser
            {
                Id = identityUser.Id,
                Email = identityUser.Email!,
                UserName = identityUser.UserName!
            };

            var roles = await _userManager.GetRolesAsync(identityUser);
            var newAccessToken = await _jwtTokenGenerator.GenerateTokenAsync(jwtUser, roles);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var newRefreshTokenHash = _jwtTokenGenerator.HashToken(newRefreshToken);

            await _jwtTokenGenerator.SaveRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = identityUser.Id.ToString(),
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ReplacedByTokenHash = newRefreshTokenHash
            });

            return ServiceResponse<LoginResponse>.Ok(
                new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Role = roles.FirstOrDefault() ?? "User",
                    UserId = identityUser.Id,
                    FullName = identityUser.FullName ?? identityUser.UserName ?? string.Empty,
                    Email = identityUser.Email ?? string.Empty
                },
                "Token refreshed successfully.");
        }

        public async Task<ServiceResponse<bool>> RevokeTokenAsync(string userId)
        {
            await _jwtTokenGenerator.RevokeAllUserTokensAsync(userId);
            return ServiceResponse<bool>.Ok(true, "All tokens revoked successfully.");
        }

        // ─────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────

        /// <summary>Creates an ASP.NET Identity user and assigns the given role.</summary>
        /// <returns>The new user's <see cref="Guid"/> ID.</returns>
        /// <exception cref="ConflictException">When the email is already taken.</exception>
        /// <exception cref="RegistrationFailedException">When Identity reports validation errors.</exception>
        private async Task<Guid> CreateIdentityUserAsync(RegisterBaseRequest request, string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
                throw new ConflictException($"An account with email '{request.Email}' already exists.");

            var user = new ApplicationIdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.Phone,
                FullName = request.FullName,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new RegistrationFailedException(errors);
            }

            await _userManager.AddToRoleAsync(user, role);

            return Guid.Parse(user.Id.ToString());
        }
    }
}
