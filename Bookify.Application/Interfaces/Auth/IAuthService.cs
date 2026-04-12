using Bookify.Application.Common;
using Bookify.Application.DTO.Auth;
using Bookify.Application.DTO.Identity;

namespace Bookify.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<ServiceResponse<bool>> InitiateRegisterClientAsync(RegisterClientRequest request);
        Task<ServiceResponse<bool>> InitiateRegisterStaffAsync(RegisterStaffRequest request);
        Task<ServiceResponse<Guid>> VerifyRegistrationOtpAsync(VerifyRegistrationOtpRequest request);
        Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ServiceResponse<bool>> RevokeTokenAsync(string userId);

        // Forgot Password
        Task<ServiceResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ServiceResponse<string>> VerifyOtpAsync(VerifyOtpRequest request); // Returns ResetToken
        Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
