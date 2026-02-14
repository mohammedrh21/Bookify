using Bookify.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Interfaces
{
    public interface IIdentityUserService
    {
        Task<ServiceResponse<string>> CreateUserAsync(
            string email,
            string password,
            string phone,
            string role);
    }
}
