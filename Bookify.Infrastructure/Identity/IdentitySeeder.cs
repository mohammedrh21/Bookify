using Bookify.Application.Interfaces;
using Bookify.Domain.Contracts;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Identity.Entity;
using Bookify.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Identity
{
    public sealed class IdentitySeeder : IIdentitySeeder
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<Admin> _adminRepo;

        private static readonly string[] Roles =
        {
            "Admin",
            "Staff",
            "Client"
        };

        public IdentitySeeder(
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<ApplicationIdentityUser> userManager,
            IConfiguration configuration,
            IGenericRepository<Admin> adminRepo)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _adminRepo = adminRepo;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedAdminAsync();
        }

        private async Task SeedRolesAsync()
        {
            foreach (var role in Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }

        private async Task SeedAdminAsync()
        {
            var adminEmail = _configuration["Admin:Email"];
            var adminPassword = _configuration["Admin:Password"];
            var adminName = _configuration["Admin:FullName"];
            var adminPhone = _configuration["Admin:PhoneNumber"];


            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword) ||
                string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminPhone))
                return;

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null)
                return;

            adminUser = new ApplicationIdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminName,
                PhoneNumber = adminPhone,
                PhoneNumberConfirmed = true,
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var adminTableUser = new Admin()
            {
                Id = adminUser.Id,
                FullName = adminUser.FullName,
                Phone = adminUser.PhoneNumber
            };

            await _adminRepo.AddAsync(adminTableUser);
        }
    }
}