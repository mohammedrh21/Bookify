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
            var identityUser = await CreateIdentityUser(request, "Client");
            if (!identityUser.Success)
                return ServiceResponse<Guid>.Fail(identityUser.Message!);

            var client = new Client
            {
                Id = Guid.NewGuid(),
                IdentityUserId = identityUser.Data!,
                FullName = request.FullName,
                Phone = request.Phone
            };

            await _clientRepo.AddAsync(client);

            return ServiceResponse<Guid>.Ok(id: client.Id, message: "Client registered");
        }

        public async Task<ServiceResponse<Guid>> RegisterStaffAsync(RegisterStaffRequest request)
        {
            var identityUser = await CreateIdentityUser(request, "Staff");
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

            return ServiceResponse<Guid>.Ok(id: staff.Id, message: "Staff registered");
        }

        private async Task<ServiceResponse<string>> CreateIdentityUser(
            RegisterBaseRequest request,
            string role)
        {
            var user = new ApplicationIdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.Phone
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return ServiceResponse<string>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, role);

            return ServiceResponse<string>.Ok(user.Id);
        }

    public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            // Find user via Identity (Infrastructure concern)
            var identityUser = await _userManager.FindByEmailAsync(request.Email);
            if (identityUser is null)
                return ServiceResponse<LoginResponse>.Fail("Invalid email or password");

            if (!await _userManager.CheckPasswordAsync(identityUser, request.Password))
                return ServiceResponse<LoginResponse>.Fail("Invalid email or password");

            // Map IdentityUser → JwtUser
            var jwtUser = new JwtUser
            {
                Id = identityUser.Id,
                Email = identityUser.Email!,
                UserName = identityUser.UserName!
            };

            var roles = await _userManager.GetRolesAsync(identityUser);
            var token = await _jwtTokenGenerator.GenerateTokenAsync(jwtUser, roles);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
            var hash = _jwtTokenGenerator.HashToken(refreshToken);

            await _jwtTokenGenerator.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });
            return ServiceResponse<LoginResponse>.Ok(new LoginResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1),
                Role = roles.FirstOrDefault() ?? "User",
                UserId = Guid.Parse(identityUser.Id)
            }, "Login successful");
        }
    }
}
