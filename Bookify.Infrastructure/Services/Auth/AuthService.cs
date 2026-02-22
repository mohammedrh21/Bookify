using Bookify.Application.Common;
using Bookify.Application.DTO;
using Bookify.Application.DTO.Auth;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.Auth;
using Bookify.Application.Interfaces.Client;
using Bookify.Application.Interfaces.Staff;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Services.Auth
{
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

        public async Task<ServiceResponse<Guid>> RegisterClientAsync(RegisterClientRequest request)
        {
            var identityUser = await CreateIdentityUser(request, Roles.Client);
            if (!identityUser.Success)
                return ServiceResponse<Guid>.Fail(identityUser.Message!);

            var client = new Client
            {
                Id = Guid.NewGuid(),
                IdentityUserId = identityUser.Data!,
                FullName = request.FullName,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth
            };

            await _clientRepo.AddAsync(client);

            return ServiceResponse<Guid>.Ok(
                id: client.Id,
                message: "Client registered successfully");
        }

        public async Task<ServiceResponse<Guid>> RegisterStaffAsync(RegisterStaffRequest request)
        {
            var identityUser = await CreateIdentityUser(request, Roles.Staff);
            if (!identityUser.Success)
                return ServiceResponse<Guid>.Fail(identityUser.Message!);

            var staff = new Staff
            {
                Id = Guid.NewGuid(),
                IdentityUserId = identityUser.Data!,
                FullName = request.FullName,
                Phone = request.Phone
            };

            await _staffRepo.AddAsync(staff);

            return ServiceResponse<Guid>.Ok(
                id: staff.Id,
                message: "Staff registered successfully");
        }

        private async Task<ServiceResponse<string>> CreateIdentityUser(
            RegisterBaseRequest request,
            string role)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ServiceResponse<string>.Fail("A user with this email already exists");
            }

            var user = new ApplicationIdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.Phone,
                FullName = request.FullName,
                EmailConfirmed = false // Require email confirmation in production
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResponse<string>.Fail(errors);
            }

            await _userManager.AddToRoleAsync(user, role);

            return ServiceResponse<string>.Ok(user.Id, "User created successfully");
        }

        public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            // Find user via Identity
            var identityUser = await _userManager.FindByEmailAsync(request.Email);
            if (identityUser is null)
                return ServiceResponse<LoginResponse>.Fail("Invalid email or password");

            // Check password
            if (!await _userManager.CheckPasswordAsync(identityUser, request.Password))
                return ServiceResponse<LoginResponse>.Fail("Invalid email or password");

            // Check if email is confirmed (optional - enable in production)
            // if (!identityUser.EmailConfirmed)
            //     return ServiceResponse<LoginResponse>.Fail("Please confirm your email address");

            // Check if user is locked out
            if (await _userManager.IsLockedOutAsync(identityUser))
                return ServiceResponse<LoginResponse>.Fail("Account is locked. Please try again later");

            // Map IdentityUser → JwtUser
            var jwtUser = new JwtUser
            {
                Id = identityUser.Id,
                Email = identityUser.Email!,
                UserName = identityUser.UserName!
            };

            // Get user roles
            var roles = await _userManager.GetRolesAsync(identityUser);

            // Generate access token
            var accessToken = await _jwtTokenGenerator.GenerateTokenAsync(jwtUser, roles);

            // Generate refresh token
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenHash = _jwtTokenGenerator.HashToken(refreshToken);

            // Save refresh token to database
            await _jwtTokenGenerator.SaveRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = identityUser.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            });

            // Reset access failed count on successful login
            await _userManager.ResetAccessFailedCountAsync(identityUser);

            return ServiceResponse<LoginResponse>.Ok(
                new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Role = roles.FirstOrDefault() ?? "User",
                    UserId = Guid.Parse(identityUser.Id),
                    FullName = identityUser.FullName ?? identityUser.UserName ?? string.Empty,
                    Email = identityUser.Email ?? string.Empty
                },
                "Login successful");
        }

        public async Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var tokenHash = _jwtTokenGenerator.HashToken(request.RefreshToken);
            var storedToken = await _jwtTokenGenerator.ValidateRefreshTokenAsync(tokenHash);

            if (storedToken == null)
                return ServiceResponse<LoginResponse>.Fail("Invalid or expired refresh token");

            // Get user
            var identityUser = await _userManager.FindByIdAsync(storedToken.UserId);
            if (identityUser == null)
                return ServiceResponse<LoginResponse>.Fail("User not found");

            // Revoke old refresh token
            await _jwtTokenGenerator.RevokeTokenAsync(tokenHash);

            // Generate new tokens
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

            // Save new refresh token
            await _jwtTokenGenerator.SaveRefreshTokenAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = identityUser.Id,
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
                    UserId = Guid.Parse(identityUser.Id),
                    FullName = identityUser.FullName ?? identityUser.UserName ?? string.Empty,
                    Email = identityUser.Email ?? string.Empty
                },
                "Token refreshed successfully");
        }

        public async Task<ServiceResponse<bool>> RevokeTokenAsync(string userId)
        {
            await _jwtTokenGenerator.RevokeAllUserTokensAsync(userId);
            return ServiceResponse<bool>.Ok(true, "All tokens revoked successfully");
        }
    }
}

