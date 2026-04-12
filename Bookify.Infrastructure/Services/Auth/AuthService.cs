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
using Bookify.Application.Interfaces.Email;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

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
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;

        public AuthService(
            UserManager<ApplicationIdentityUser> userManager,
            IClientRepository clientRepo,
            IStaffRepository staffRepo,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailSender emailSender,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _clientRepo = clientRepo;
            _staffRepo = staffRepo;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailSender = emailSender;
            _cache = cache;
        }

        // ─────────────────────────────────────────────
        // Registration
        // ─────────────────────────────────────────────

        private class CachedRegistration
        {
            public RegisterClientRequest? ClientRequest { get; set; }
            public RegisterStaffRequest? StaffRequest { get; set; }
            public string Otp { get; set; } = string.Empty;
        }

        private async Task<ServiceResponse<bool>> InitiateRegistrationAsync(
            string email, string otp, CachedRegistration cachedRegistration)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                throw new ConflictException($"An account with email '{email}' already exists.");

            var cacheKey = $"Registration_OTP_{email.ToLower()}";
            _cache.Set(cacheKey, cachedRegistration, TimeSpan.FromMinutes(5));

            var subject = "Your Registration OTP";
            var message = $"<p>Your OTP for completing registration is: <strong>{otp}</strong></p><p>This OTP will expire in 5 minutes.</p>";

            await _emailSender.SendEmailAsync(email, subject, message);

            return ServiceResponse<bool>.Ok(true, "OTP sent to your email. Please verify to complete registration.");
        }

        public async Task<ServiceResponse<bool>> InitiateRegisterClientAsync(RegisterClientRequest request)
        {
            var otp = GenerateOtp();
            var cacheModel = new CachedRegistration { ClientRequest = request, Otp = otp };
            return await InitiateRegistrationAsync(request.Email, otp, cacheModel);
        }

        public async Task<ServiceResponse<bool>> InitiateRegisterStaffAsync(RegisterStaffRequest request)
        {
            var otp = GenerateOtp();
            var cacheModel = new CachedRegistration { StaffRequest = request, Otp = otp };
            return await InitiateRegistrationAsync(request.Email, otp, cacheModel);
        }

        /// <summary>Generates a cryptographically random 6-digit OTP.</summary>
        private static string GenerateOtp()
            => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        public async Task<ServiceResponse<Guid>> VerifyRegistrationOtpAsync(VerifyRegistrationOtpRequest request)
        {
            var cacheKey = $"Registration_OTP_{request.Email.ToLower()}";
            if (!_cache.TryGetValue<CachedRegistration>(cacheKey, out var cachedRegistration) || cachedRegistration is null)
                throw new BusinessRuleException("Registration session expired or does not exist. Please register again.");

            if (cachedRegistration.Otp != request.Otp)
                throw new BusinessRuleException("Invalid OTP.");

            Guid userId;
            try
            {
                if (cachedRegistration.ClientRequest is { } clientReq)
                {
                    userId = await CreateIdentityUserAsync(clientReq, Roles.Client);
                    await _clientRepo.AddAsync(new Client
                    {
                        Id          = userId,
                        FullName    = clientReq.FullName,
                        Phone       = clientReq.Phone,
                        DateOfBirth = clientReq.DateOfBirth
                    });
                }
                else if (cachedRegistration.StaffRequest is { } staffReq)
                {
                    userId = await CreateIdentityUserAsync(staffReq, Roles.Staff);
                    await _staffRepo.AddAsync(new Staff
                    {
                        Id       = userId,
                        FullName = staffReq.FullName,
                        Phone    = staffReq.Phone
                    });
                }
                else
                {
                    throw new BusinessRuleException("Corrupted registration session. Please register again.");
                }
            }
            finally
            {
                // Always evict from cache — prevents re-use of a consumed or failed OTP
                _cache.Remove(cacheKey);
            }

            return ServiceResponse<Guid>.Ok(userId, "Registration completed successfully.");
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
            var user = new ApplicationIdentityUser
            {
                UserName      = request.Email,
                Email         = request.Email,
                PhoneNumber   = request.Phone,
                FullName      = request.FullName,
                EmailConfirmed = true   // OTP already verified — email is confirmed
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

        // ─────────────────────────────────────────────
        // Forgot Password
        // ─────────────────────────────────────────────

        public async Task<ServiceResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                throw new BusinessRuleException("This email is not registered yet.");
            }

            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            user.ResetOtp = otp;
            user.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            await _userManager.UpdateAsync(user);

            var subject = "Your Password Reset OTP";
            var message = $"<p>Your OTP for password reset is: <strong>{otp}</strong></p><p>This OTP will expire in 5 minutes.</p>";

            await _emailSender.SendEmailAsync(request.Email, subject, message);

            return ServiceResponse<bool>.Ok(true, "OTP sent to your email.");
        }

        public async Task<ServiceResponse<string>> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                throw new BusinessRuleException("Invalid request.");

            if (user.ResetOtp != request.Otp)
                throw new BusinessRuleException("Invalid OTP.");

            if (user.ResetOtpExpiry < DateTime.UtcNow)
                throw new BusinessRuleException("OTP expired.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            user.ResetOtp = null;
            user.ResetOtpExpiry = null;
            await _userManager.UpdateAsync(user);

            return ServiceResponse<string>.Ok(token, "OTP verified successfully.");
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                throw new NotFoundException("User", request.Email);

            var result = await _userManager.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BusinessRuleException($"Password reset failed: {errors}");
            }

            return ServiceResponse<bool>.Ok(true, "Password reset successfully.");
        }
    }
}
