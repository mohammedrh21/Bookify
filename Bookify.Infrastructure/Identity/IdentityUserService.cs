using Bookify.Application.Common;
using Bookify.Application.DTO.Identity;
using Bookify.Application.Interfaces;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Identity
{
    public class IdentityUserService : IIdentityUserService
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly AppDbContext _db;
        private readonly IFileService _fileService;

        public IdentityUserService(
            UserManager<ApplicationIdentityUser> userManager,
            AppDbContext db,
            IFileService fileService)
        {
            _userManager = userManager;
            _db = db;
            _fileService = fileService;
        }

        // ── Create ────────────────────────────────────────────────────────────

        public async Task<ServiceResponse<string>> CreateUserAsync(
            string email,
            string password,
            string phone,
            string role)
        {
            var user = new ApplicationIdentityUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = phone,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return ServiceResponse<string>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, role);

            return ServiceResponse<string>.Ok(user.Id.ToString());
        }

        // ── Client Profile ───────────────────────────────────────────────────

        public async Task<ServiceResponse<ClientProfileResponse>> GetClientProfileAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var guid))
                return ServiceResponse<ClientProfileResponse>.Fail("Invalid user ID.");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser is null)
                return ServiceResponse<ClientProfileResponse>.Fail("User not found.");

            var profile = new ClientProfileResponse
            {
                Id = guid,
                FullName = identityUser.FullName,
                Email = identityUser.Email ?? string.Empty,
                Phone = identityUser.PhoneNumber ?? string.Empty,
                Role = "Client"
            };

            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == guid);
            if (client is not null)
            {
                profile.FullName = client.FullName;
                profile.Phone = client.Phone;
                profile.Gender = client.Gender;
                profile.DateOfBirth = client.DateOfBirth;
                profile.ImagePath = client.ImagePath;
            }

            return ServiceResponse<ClientProfileResponse>.Ok(profile);
        }

        public async Task<ServiceResponse<bool>> UpdateClientProfileAsync(string userId, UpdateClientProfileRequest request)
        {
            if (!Guid.TryParse(userId, out var guid))
                return ServiceResponse<bool>.Fail("Invalid user ID.");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser is null)
                return ServiceResponse<bool>.Fail("User not found.");

            identityUser.FullName = request.FullName;
            identityUser.PhoneNumber = request.Phone;
            var identityResult = await _userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
                return ServiceResponse<bool>.Fail(
                    string.Join(", ", identityResult.Errors.Select(e => e.Description)));

            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == guid);
            if (client is not null)
            {
                client.FullName = request.FullName;
                client.Phone = request.Phone;
                client.Gender = request.Gender ?? client.Gender;
                client.DateOfBirth = request.DateOfBirth ?? client.DateOfBirth;
                await _db.SaveChangesAsync();
            }

            return ServiceResponse<bool>.Ok(true, "Client profile updated successfully.");
        }

        // ── Staff Profile ────────────────────────────────────────────────────

        public async Task<ServiceResponse<StaffProfileResponse>> GetStaffProfileAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var guid))
                return ServiceResponse<StaffProfileResponse>.Fail("Invalid user ID.");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser is null)
                return ServiceResponse<StaffProfileResponse>.Fail("User not found.");

            var profile = new StaffProfileResponse
            {
                Id = guid,
                FullName = identityUser.FullName,
                Email = identityUser.Email ?? string.Empty,
                Phone = identityUser.PhoneNumber ?? string.Empty,
                Role = "Staff"
            };

            var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.Id == guid);
            if (staff is not null)
            {
                profile.FullName = staff.FullName;
                profile.Phone = staff.Phone;
                profile.Gender = staff.Gender;
                profile.ImagePath = staff.ImagePath;
            }

            return ServiceResponse<StaffProfileResponse>.Ok(profile);
        }

        public async Task<ServiceResponse<bool>> UpdateStaffProfileAsync(string userId, UpdateStaffProfileRequest request)
        {
            if (!Guid.TryParse(userId, out var guid))
                return ServiceResponse<bool>.Fail("Invalid user ID.");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser is null)
                return ServiceResponse<bool>.Fail("User not found.");

            identityUser.FullName = request.FullName;
            identityUser.PhoneNumber = request.Phone;
            var identityResult = await _userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
                return ServiceResponse<bool>.Fail(
                    string.Join(", ", identityResult.Errors.Select(e => e.Description)));

            var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.Id == guid);
            if (staff is not null)
            {
                staff.FullName = request.FullName;
                staff.Phone = request.Phone;
                staff.Gender = request.Gender ?? staff.Gender;
                await _db.SaveChangesAsync();
            }

            return ServiceResponse<bool>.Ok(true, "Staff profile updated successfully.");
        }

        // ── Admin Profile ────────────────────────────────────────────────────

        public async Task<ServiceResponse<AdminProfileResponse>> GetAdminProfileAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var guid))
                return ServiceResponse<AdminProfileResponse>.Fail("Invalid user ID.");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser is null)
                return ServiceResponse<AdminProfileResponse>.Fail("User not found.");

            var profile = new AdminProfileResponse
            {
                Id = guid,
                FullName = identityUser.FullName,
                Email = identityUser.Email ?? string.Empty,
                Phone = identityUser.PhoneNumber ?? string.Empty,
                Role = "Admin"
            };

            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Id == guid);
            if (admin is not null)
            {
                profile.FullName = admin.FullName;
                profile.Phone = admin.Phone;
            }

            return ServiceResponse<AdminProfileResponse>.Ok(profile);
        }

        public async Task<ServiceResponse<bool>> UpdateAdminProfileAsync(string userId, UpdateAdminProfileRequest request)
        {
            if (!Guid.TryParse(userId, out var guid))
                return ServiceResponse<bool>.Fail("Invalid user ID.");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser is null)
                return ServiceResponse<bool>.Fail("User not found.");

            identityUser.FullName = request.FullName;
            identityUser.PhoneNumber = request.Phone;
            var identityResult = await _userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
                return ServiceResponse<bool>.Fail(
                    string.Join(", ", identityResult.Errors.Select(e => e.Description)));

            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Id == guid);
            if (admin is not null)
            {
                admin.FullName = request.FullName;
                admin.Phone = request.Phone;
                await _db.SaveChangesAsync();
            }

            return ServiceResponse<bool>.Ok(true, "Admin profile updated successfully.");
        }

        // ── Password — Change ─────────────────────────────────────────────────

        public async Task<ServiceResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return ServiceResponse<bool>.Fail("New password and confirmation do not match.");

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser is null)
                return ServiceResponse<bool>.Fail("User not found.");

            var result = await _userManager.ChangePasswordAsync(
                identityUser, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
                return ServiceResponse<bool>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            return ServiceResponse<bool>.Ok(true, "Password changed successfully.");
        }

        // ── User Status ──────────────────────────────────────────────────────

        public async Task<ServiceResponse<bool>> ToggleUserActiveAsync(Guid userId)
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser is null)
                return ServiceResponse<bool>.Fail("User not found.");

            identityUser.IsActive = !identityUser.IsActive;
            var result = await _userManager.UpdateAsync(identityUser);

            if (!result.Succeeded)
                return ServiceResponse<bool>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            return ServiceResponse<bool>.Ok(identityUser.IsActive, "User active status toggled successfully.");
        }

        public async Task<ServiceResponse<string>> UpdateProfileImageAsync(string userId, IFormFile file, string role)
        {
            if (!Guid.TryParse(userId, out var guid))
                return ServiceResponse<string>.Fail("Invalid user ID.");

            var folderName = role == "Staff" ? "Staff" : "Client";
            var extension = Path.GetExtension(file.FileName);
            var customFileName = $"{guid}{extension}";

            try
            {
                var imageUrl = await _fileService.Upload(file, folderName, customFileName);

                if (role == "Staff")
                {
                    var staff = await _db.Staffs.FirstOrDefaultAsync(s => s.Id == guid);
                    if (staff != null)
                    {
                        staff.ImagePath = imageUrl;
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == guid);
                    if (client != null)
                    {
                        client.ImagePath = imageUrl;
                        await _db.SaveChangesAsync();
                    }
                }

                return ServiceResponse<string>.Ok(imageUrl, "Profile image updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail(ex.Message);
            }
        }
    }
}
