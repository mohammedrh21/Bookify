using Bookify.Application.Common;
using Bookify.Application.DTO;
using Bookify.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<ServiceResponse<Guid>> RegisterClientAsync(RegisterClientRequest request);
        Task<ServiceResponse<Guid>> RegisterStaffAsync(RegisterStaffRequest request);
        Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request);

    }
}
