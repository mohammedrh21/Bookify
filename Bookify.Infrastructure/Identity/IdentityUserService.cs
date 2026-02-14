using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Infrastructure.Identity
{
    public class IdentityUserService : IIdentityUserService
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public IdentityUserService(UserManager<ApplicationIdentityUser> userManager)
        {
            _userManager = userManager;
        }

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

            return ServiceResponse<string>.Ok(user.Id);
        }
    }

}
