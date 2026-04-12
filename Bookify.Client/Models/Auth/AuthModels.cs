using System.ComponentModel.DataAnnotations;

namespace Bookify.Client.Models.Auth;

public class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string    FullName    { get; set; } = string.Empty;
    public string    Email       { get; set; } = string.Empty;
    public string    Password    { get; set; } = string.Empty;
    [Required, Phone]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 15 digits.")]
    public string    Phone       { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
}

public class RegisterStaffRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    [Required, Phone]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 15 digits.")]
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// Matches the API's <c>LoginResponse</c> DTO (nested inside <c>ServiceResponse&lt;T&gt;</c>).
/// </summary>
public class LoginResponseModel
{
    public string   AccessToken  { get; set; } = string.Empty;
    public string   RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration   { get; set; }
    public string   Role         { get; set; } = string.Empty;
    public Guid     UserId       { get; set; }
    public string   FullName     { get; set; } = string.Empty;
    public string   Email        { get; set; } = string.Empty;
}

public class ForgotPasswordRequestModel
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpRequestModel
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class ResetPasswordRequestModel
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyRegistrationOtpRequestModel
{
    public string Email { get; set; } = string.Empty;
    public string Otp   { get; set; } = string.Empty;
}
