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
    public string    Phone       { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
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
