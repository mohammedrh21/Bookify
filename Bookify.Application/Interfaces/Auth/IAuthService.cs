using Bookify.Application.Common;
using Bookify.Application.DTO.Auth;
using Bookify.Application.DTO.Identity;
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
        Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ServiceResponse<bool>> RevokeTokenAsync(string userId);

        // Forgot Password
        Task<ServiceResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ServiceResponse<string>> VerifyOtpAsync(VerifyOtpRequest request); // Returns ResetToken
        Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
