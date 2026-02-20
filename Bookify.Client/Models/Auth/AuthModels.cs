namespace Bookify.Client.Models.Auth;

public class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone    { get; set; } = string.Empty;
    public string Role     { get; set; } = "Client";
}

public class AuthResponse
{
    public bool   IsSuccess    { get; set; }
    public string AccessToken  { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string FullName     { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string Role         { get; set; } = string.Empty;
    public string? Message     { get; set; }
}
